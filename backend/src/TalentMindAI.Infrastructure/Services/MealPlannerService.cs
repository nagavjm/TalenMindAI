using Microsoft.EntityFrameworkCore;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class MealPlannerService : IMealPlannerService
{
    private readonly AppDbContext _dbContext;
    private readonly IFoodAiCompletionService _foodAi;
    private readonly IFoodResponsibleAiService _responsibleAi;
    private readonly IPromptLogService _promptLog;

    public MealPlannerService(AppDbContext dbContext, IFoodAiCompletionService foodAi, IFoodResponsibleAiService responsibleAi, IPromptLogService promptLog)
    {
        _dbContext = dbContext;
        _foodAi = foodAi;
        _responsibleAi = responsibleAi;
        _promptLog = promptLog;
    }

    public async Task<MealPlanDto> GenerateAsync(Guid userId, MealPlanGenerateRequest request, CancellationToken ct = default)
    {
        var inputText = $"{request.Title}\n{request.Notes}";
        var inputCheck = await _responsibleAi.ValidateUserInputAsync(inputText, ct);
        if (!inputCheck.IsSafe)
        {
            await _promptLog.LogAsync(userId, "MealPlanGenerate", inputText, string.Empty, containedPii: false, failedSafety: true, injectionDetected: inputCheck.InjectionDetected, latencyMs: 0, ct);
            throw new InvalidOperationException(inputCheck.Reason ?? "Potential prompt injection or unsafe content detected in the request.");
        }

        const string systemPrompt = "You are a meal-planning assistant for TalentMind NutriAI. Respond ONLY with a JSON object representing a day-by-day meal plan (breakfast, lunch, dinner, snacks) between the given start and end dates, respecting dietary tags.";
        var userPrompt = $"Title: {request.Title}\nStart: {request.StartDate:yyyy-MM-dd}\nEnd: {request.EndDate:yyyy-MM-dd}\nDietary tags: {string.Join(", ", request.DietaryTags ?? new List<string>())}\nNotes: {request.Notes}";

        var raw = await _foodAi.CompleteAsync(systemPrompt, userPrompt, jsonMode: true, ct);

        var outputCheck = await _responsibleAi.ValidateAiOutputAsync(raw, ct);
        if (!outputCheck.IsSafe)
        {
            await _promptLog.LogAsync(userId, "MealPlanGenerate", userPrompt, raw, containedPii: false, failedSafety: true, injectionDetected: false, latencyMs: 0, ct);
            throw new InvalidOperationException(outputCheck.Reason ?? "The generated meal plan was flagged as unsafe. Please try different preferences.");
        }

        await _promptLog.LogAsync(userId, "MealPlanGenerate", userPrompt, raw, containedPii: false, failedSafety: false, injectionDetected: false, latencyMs: 0, ct);

        var entity = new MealPlan
        {
            UserId = userId,
            Title = request.Title,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PlanJson = raw,
            Notes = request.Notes,
            GeneratedByAi = true
        };

        _dbContext.MealPlans.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<MealPlanListResponse> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var plans = await _dbContext.MealPlans
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return new MealPlanListResponse(plans.Select(ToDto).ToList());
    }

    private static MealPlanDto ToDto(MealPlan m) => new(m.Id, m.Title, m.StartDate, m.EndDate, m.PlanJson, m.Notes, m.GeneratedByAi, m.CreatedAt);
}
