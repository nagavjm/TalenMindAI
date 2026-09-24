using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class PromptLogService : IPromptLogService
{
    private readonly AppDbContext _dbContext;

    public PromptLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(Guid? userId, string promptType, string prompt, string response, bool containedPii, bool failedSafety, bool injectionDetected, int latencyMs, CancellationToken ct = default)
    {
        _dbContext.PromptLogs.Add(new PromptLog
        {
            UserId = userId,
            PromptType = promptType,
            Prompt = prompt,
            Response = response,
            ContainedPii = containedPii,
            FailedContentSafety = failedSafety,
            PromptInjectionDetected = injectionDetected,
            LatencyMs = latencyMs
        });

        await _dbContext.SaveChangesAsync(ct);
    }
}
