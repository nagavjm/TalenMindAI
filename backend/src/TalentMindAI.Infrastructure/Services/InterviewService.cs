using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class InterviewService : IInterviewService
{
    private readonly AppDbContext _dbContext;
    private readonly IAiFoundryService _aiFoundry;

    public InterviewService(AppDbContext dbContext, IAiFoundryService aiFoundry)
    {
        _dbContext = dbContext;
        _aiFoundry = aiFoundry;
    }

    public async Task<InterviewQuestionResponse> GenerateAsync(InterviewQuestionRequest request, CancellationToken ct = default)
    {
        var resume = await _dbContext.Resumes.Include(r => r.Analysis).FirstOrDefaultAsync(r => r.Id == request.ResumeId, ct)
            ?? throw new KeyNotFoundException("Resume not found.");

        var skills = resume.Analysis is null
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(resume.Analysis.TechnicalSkillsJson) ?? new();

        const string systemPrompt = "You are a technical interviewer. Generate interview questions for the given skills, categorized as Beginner, Intermediate, and Advanced. Return JSON array of {question, difficulty, skill}.";
        var userPrompt = $"Skills: {string.Join(", ", skills)}";

        var rawResponse = await _aiFoundry.CompleteAsync(systemPrompt, userPrompt, ct);

        List<InterviewQuestionDto> questions;
        try
        {
            questions = JsonSerializer.Deserialize<List<InterviewQuestionDto>>(rawResponse) ?? new();
        }
        catch (JsonException)
        {
            questions = new List<InterviewQuestionDto>();
        }

        return new InterviewQuestionResponse(resume.Id, questions);
    }
}
