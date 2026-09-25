namespace TalentMindAI.Application.DTOs;

// ---------- Food Chat (TalentMind NutriAI) ----------
public record FoodChatQueryRequest(string Question);

public record FoodChatQueryResponse(Guid ChatHistoryId, string Answer, List<string> SourceChunks);

public record FoodChatFeedbackRequest(Guid ChatHistoryId, bool IsHelpful);

// ---------- Recipes ----------
public record RecipeGenerateRequest(string Prompt, List<string>? Ingredients, string? Cuisine, List<string>? DietaryTags, int? Servings);

public record RecipeDto(
    Guid Id,
    string Title,
    string Description,
    List<string> Ingredients,
    string Instructions,
    string Cuisine,
    List<string> DietaryTags,
    int? CaloriesPerServing,
    int PrepTimeMinutes,
    int Servings,
    bool GeneratedByAi,
    DateTime CreatedAt);

public record RecipeListResponse(List<RecipeDto> Recipes);

// ---------- Nutrition ----------
public record NutritionAnalyzeRequest(string FoodDescription);

public record NutritionAnalysisDto(
    Guid Id,
    string FoodDescription,
    double? Calories,
    double? ProteinGrams,
    double? CarbsGrams,
    double? FatGrams,
    DateTime CreatedAt);

// ---------- Meal Planner ----------
public record MealPlanGenerateRequest(string Title, DateTime StartDate, DateTime EndDate, List<string>? DietaryTags, string? Notes);

public record MealPlanDto(Guid Id, string Title, DateTime StartDate, DateTime EndDate, string PlanJson, string? Notes, bool GeneratedByAi, DateTime CreatedAt);

public record MealPlanListResponse(List<MealPlanDto> Plans);
