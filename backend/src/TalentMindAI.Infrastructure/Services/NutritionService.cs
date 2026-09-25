using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class NutritionService : INutritionService
{
    private readonly AppDbContext _dbContext;
    private readonly IFoodAiCompletionService _foodAi;
    private readonly IPromptLogService _promptLog;

    public NutritionService(AppDbContext dbContext, IFoodAiCompletionService foodAi, IPromptLogService promptLog)
    {
        _dbContext = dbContext;
        _foodAi = foodAi;
        _promptLog = promptLog;
    }

    public async Task<NutritionAnalysisDto> AnalyzeAsync(Guid userId, NutritionAnalyzeRequest request, CancellationToken ct = default)
    {
        const string systemPrompt = "You are a nutrition analysis assistant for TalentMind NutriAI. Respond ONLY with a JSON object with keys: calories, proteinGrams, carbsGrams, fatGrams (all numbers, estimate if needed), and summary (string). This is an estimate, not medical advice.";
        var userPrompt = $"Food description: {request.FoodDescription}";

        var raw = await _foodAi.CompleteAsync(systemPrompt, userPrompt, jsonMode: true, ct);
        await _promptLog.LogAsync(userId, "NutritionAnalyze", userPrompt, raw, containedPii: false, failedSafety: false, injectionDetected: false, latencyMs: 0, ct);

        var (calories, protein, carbs, fat) = ParseMacros(raw);

        var entity = new NutritionAnalysis
        {
            UserId = userId,
            FoodDescription = request.FoodDescription,
            Calories = calories,
            ProteinGrams = protein,
            CarbsGrams = carbs,
            FatGrams = fat,
            AnalysisJson = raw
        };

        _dbContext.NutritionAnalyses.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<List<NutritionAnalysisDto>> GetHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _dbContext.NutritionAnalyses
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

        return items.Select(ToDto).ToList();
    }

    private static NutritionAnalysisDto ToDto(NutritionAnalysis n) => new(
        n.Id, n.FoodDescription, n.Calories, n.ProteinGrams, n.CarbsGrams, n.FatGrams, n.CreatedAt);

    private static (double? calories, double? protein, double? carbs, double? fat) ParseMacros(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            double? Get(string name) => root.TryGetProperty(name, out var v) && v.TryGetDouble(out var d) ? d : null;
            return (Get("calories"), Get("proteinGrams"), Get("carbsGrams"), Get("fatGrams"));
        }
        catch (JsonException)
        {
            return (null, null, null, null);
        }
    }
}
