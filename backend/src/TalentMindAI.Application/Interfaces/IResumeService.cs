using Microsoft.AspNetCore.Http;
using TalentMindAI.Application.DTOs;

namespace TalentMindAI.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeUploadResponse> UploadAsync(Guid userId, IFormFile file, CancellationToken ct = default);
    Task<ResumeAnalysisDto> AnalyzeAsync(Guid resumeId, CancellationToken ct = default);
    Task<CandidateDetailsDto?> GetCandidateDetailsAsync(Guid resumeId, CancellationToken ct = default);
    Task<List<ResumeSummaryDto>> GetResumesAsync(Guid? userId, CancellationToken ct = default);
}
