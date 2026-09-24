using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Domain.Enums;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class ResumeService : IResumeService
{
    private readonly AppDbContext _dbContext;
    private readonly IBlobStorageService _blobStorage;
    private readonly IOcrService _ocrService;
    private readonly IAiFoundryService _aiFoundry;
    private readonly ISearchIndexService _searchIndex;
    private readonly ILanguageService _languageService;
    private readonly IResponsibleAiService _responsibleAi;
    private readonly IPromptLogService _promptLog;

    public ResumeService(
        AppDbContext dbContext,
        IBlobStorageService blobStorage,
        IOcrService ocrService,
        IAiFoundryService aiFoundry,
        ISearchIndexService searchIndex,
        ILanguageService languageService,
        IResponsibleAiService responsibleAi,
        IPromptLogService promptLog)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
        _ocrService = ocrService;
        _aiFoundry = aiFoundry;
        _searchIndex = searchIndex;
        _languageService = languageService;
        _responsibleAi = responsibleAi;
        _promptLog = promptLog;
    }

    public async Task<ResumeUploadResponse> UploadAsync(Guid userId, IFormFile file, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileType = extension switch
        {
            ".pdf" => ResumeFileType.Pdf,
            ".docx" => ResumeFileType.Docx,
            ".jpg" or ".jpeg" => ResumeFileType.Jpg,
            ".png" => ResumeFileType.Png,
            _ => throw new NotSupportedException($"File type {extension} is not supported.")
        };

        await using var stream = file.OpenReadStream();
        var blobName = $"{userId}/{Guid.NewGuid()}{extension}";
        var blobUri = await _blobStorage.UploadAsync("resumes", blobName, stream, file.ContentType, ct);

        var resume = new Resume
        {
            UserId = userId,
            CandidateName = Path.GetFileNameWithoutExtension(file.FileName),
            FileName = file.FileName,
            FileType = fileType,
            BlobUri = blobUri,
            FileSizeBytes = file.Length,
            Status = ResumeStatus.Uploaded
        };

        if (fileType is ResumeFileType.Jpg or ResumeFileType.Png)
        {
            await using var imageStream = file.OpenReadStream();
            resume.ExtractedText = await _ocrService.ExtractTextFromImageAsync(imageStream, ct);
            resume.Status = ResumeStatus.OcrCompleted;
        }

        _dbContext.Resumes.Add(resume);
        await _dbContext.SaveChangesAsync(ct);

        return new ResumeUploadResponse(resume.Id, resume.FileName, resume.Status.ToString());
    }

    public async Task<ResumeAnalysisDto> AnalyzeAsync(Guid resumeId, CancellationToken ct = default)
    {
        var resume = await _dbContext.Resumes.Include(r => r.Analysis).FirstOrDefaultAsync(r => r.Id == resumeId, ct)
            ?? throw new KeyNotFoundException("Resume not found.");

        var content = resume.ExtractedText ?? string.Empty;
        var maskedContent = await _languageService.MaskPiiAsync(content, ct);

        const string systemPrompt = "You are an expert technical recruiter. Analyze the resume and return JSON with summary, technicalSkills, certifications, yearsOfExperience, strengths, suggestedRoles.";
        var rawResponse = await _aiFoundry.CompleteAsync(systemPrompt, maskedContent, ct);

        await _promptLog.LogAsync(resume.UserId, "Analysis", maskedContent, rawResponse, containedPii: content != maskedContent, failedSafety: false, injectionDetected: _responsibleAi.DetectPromptInjection(content), latencyMs: 0, ct);

        var keyPhrases = await _languageService.ExtractKeyPhrasesAsync(content, ct);
        var entities = await _languageService.RecognizeEntitiesAsync(content, ct);

        var analysis = resume.Analysis ?? new ResumeAnalysis { ResumeId = resume.Id };
        analysis.RawModelResponse = rawResponse;
        analysis.KeyPhrasesJson = JsonSerializer.Serialize(keyPhrases);
        analysis.NamedEntitiesJson = JsonSerializer.Serialize(entities);

        if (resume.Analysis is null)
        {
            _dbContext.Add(analysis);
        }

        resume.Status = ResumeStatus.Analyzed;
        await _dbContext.SaveChangesAsync(ct);

        await _searchIndex.IndexResumeAsync(resume.Id, resume.CandidateName, content, ct);
        resume.Status = ResumeStatus.Indexed;
        await _dbContext.SaveChangesAsync(ct);

        return new ResumeAnalysisDto(
            resume.Id,
            analysis.CandidateSummary,
            JsonSerializer.Deserialize<List<string>>(analysis.TechnicalSkillsJson) ?? new(),
            JsonSerializer.Deserialize<List<string>>(analysis.CertificationsJson) ?? new(),
            analysis.YearsOfExperience,
            JsonSerializer.Deserialize<List<string>>(analysis.StrengthsJson) ?? new(),
            JsonSerializer.Deserialize<List<string>>(analysis.SuggestedRolesJson) ?? new(),
            keyPhrases,
            entities);
    }

    public async Task<CandidateDetailsDto?> GetCandidateDetailsAsync(Guid resumeId, CancellationToken ct = default)
    {
        var resume = await _dbContext.Resumes.Include(r => r.Analysis).FirstOrDefaultAsync(r => r.Id == resumeId, ct);
        if (resume is null) return null;

        ResumeAnalysisDto? analysisDto = resume.Analysis is null
            ? null
            : new ResumeAnalysisDto(
                resume.Id,
                resume.Analysis.CandidateSummary,
                JsonSerializer.Deserialize<List<string>>(resume.Analysis.TechnicalSkillsJson) ?? new(),
                JsonSerializer.Deserialize<List<string>>(resume.Analysis.CertificationsJson) ?? new(),
                resume.Analysis.YearsOfExperience,
                JsonSerializer.Deserialize<List<string>>(resume.Analysis.StrengthsJson) ?? new(),
                JsonSerializer.Deserialize<List<string>>(resume.Analysis.SuggestedRolesJson) ?? new(),
                resume.Analysis.KeyPhrasesJson is null ? null : JsonSerializer.Deserialize<List<string>>(resume.Analysis.KeyPhrasesJson),
                resume.Analysis.NamedEntitiesJson is null ? null : JsonSerializer.Deserialize<List<string>>(resume.Analysis.NamedEntitiesJson));

        return new CandidateDetailsDto(resume.Id, resume.CandidateName, resume.FileName, resume.Status.ToString(), analysisDto);
    }

    public async Task<List<ResumeSummaryDto>> GetResumesAsync(Guid? userId, CancellationToken ct = default)
    {
        var query = _dbContext.Resumes.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ResumeSummaryDto(r.Id, r.CandidateName, r.FileName, r.Status.ToString(), r.CreatedAt))
            .ToListAsync(ct);
    }
}
