using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _dbContext;
    private readonly IFoodAiCompletionService _foodAi;
    private readonly IFoodResponsibleAiService _responsibleAi;
    private readonly IPromptLogService _promptLog;

    public RecipeService(AppDbContext dbContext, IFoodAiCompletionService foodAi, IFoodResponsibleAiService responsibleAi, IPromptLogService promptLog)
    {
        _dbContext = dbContext;
        _foodAi = foodAi;
        _responsibleAi = responsibleAi;
        _promptLog = promptLog;
    }

    public async Task<RecipeDto> GenerateAsync(Guid userId, RecipeGenerateRequest request, CancellationToken ct = default)
    {
        var inputCheck = await _responsibleAi.ValidateUserInputAsync(request.Prompt, ct);
        if (!inputCheck.IsSafe)
        {
            await _promptLog.LogAsync(userId, "RecipeGenerate", request.Prompt, string.Empty, containedPii: false, failedSafety: true, injectionDetected: inputCheck.InjectionDetected, latencyMs: 0, ct);
            throw new InvalidOperationException(inputCheck.Reason ?? "Potential prompt injection or unsafe content detected in the request.");
        }

        const string systemPrompt = "You are a recipe-writing assistant for TalentMind NutriAI. Given a prompt, ingredients, cuisine and dietary tags, respond ONLY with a JSON object with keys: title, description, ingredients (string array), instructions, caloriesPerServing (number or null), prepTimeMinutes (number).";
        var userPrompt = $"Prompt: {request.Prompt}\nIngredients: {string.Join(", ", request.Ingredients ?? new List<string>())}\nCuisine: {request.Cuisine}\nDietary tags: {string.Join(", ", request.DietaryTags ?? new List<string>())}\nServings: {request.Servings ?? 1}";

        var raw = await _foodAi.CompleteAsync(systemPrompt, userPrompt, jsonMode: true, ct);

        var outputCheck = await _responsibleAi.ValidateAiOutputAsync(raw, ct);
        if (!outputCheck.IsSafe)
        {
            await _promptLog.LogAsync(userId, "RecipeGenerate", userPrompt, raw, containedPii: false, failedSafety: true, injectionDetected: false, latencyMs: 0, ct);
            throw new InvalidOperationException(outputCheck.Reason ?? "The generated recipe content was flagged as unsafe. Please try a different prompt.");
        }

        await _promptLog.LogAsync(userId, "RecipeGenerate", userPrompt, raw, containedPii: false, failedSafety: false, injectionDetected: false, latencyMs: 0, ct);

        var generated = ParseGeneratedRecipe(raw, request);

        var entity = new FoodRecipe
        {
            UserId = userId,
            Title = generated.Title,
            Description = generated.Description,
            IngredientsJson = JsonSerializer.Serialize(generated.Ingredients),
            Instructions = generated.Instructions,
            Cuisine = request.Cuisine ?? string.Empty,
            DietaryTagsJson = JsonSerializer.Serialize(request.DietaryTags ?? new List<string>()),
            CaloriesPerServing = generated.CaloriesPerServing,
            PrepTimeMinutes = generated.PrepTimeMinutes,
            Servings = request.Servings ?? 1,
            GeneratedByAi = true
        };

        _dbContext.FoodRecipes.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<RecipeListResponse> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var recipes = await _dbContext.FoodRecipes
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return new RecipeListResponse(recipes.Select(ToDto).ToList());
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid userId, Guid recipeId, CancellationToken ct = default)
    {
        var recipe = await _dbContext.FoodRecipes.FirstOrDefaultAsync(r => r.Id == recipeId && r.UserId == userId, ct);
        return recipe is null ? null : ToDto(recipe);
    }

    private static RecipeDto ToDto(FoodRecipe r) => new(
        r.Id, r.Title, r.Description,
        JsonSerializer.Deserialize<List<string>>(r.IngredientsJson) ?? new List<string>(),
        r.Instructions, r.Cuisine,
        JsonSerializer.Deserialize<List<string>>(r.DietaryTagsJson) ?? new List<string>(),
        r.CaloriesPerServing, r.PrepTimeMinutes, r.Servings, r.GeneratedByAi, r.CreatedAt);

    private static GeneratedRecipe ParseGeneratedRecipe(string raw, RecipeGenerateRequest request)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<GeneratedRecipe>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed is not null && !string.IsNullOrWhiteSpace(parsed.Title))
            {
                return parsed;
            }
        }
        catch (JsonException)
        {
            // fall through to fallback below
        }

        return new GeneratedRecipe(
            request.Prompt,
            "AI-generated recipe.",
            request.Ingredients ?? new List<string>(),
            raw,
            null,
            30);
    }

    private record GeneratedRecipe(string Title, string Description, List<string> Ingredients, string Instructions, int? CaloriesPerServing, int PrepTimeMinutes);
}
