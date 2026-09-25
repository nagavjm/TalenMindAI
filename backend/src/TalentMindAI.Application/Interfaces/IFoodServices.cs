using TalentMindAI.Application.DTOs;

namespace TalentMindAI.Application.Interfaces;

/// <summary>
/// Dedicated AI completion abstraction for TalentMind NutriAI (Food module).
/// Backed by its own model configuration (FoodAIOptions) - never the Resume Assistant's
/// AI Foundry configuration - so the two assistants stay fully decoupled.
/// </summary>
public interface IFoodAiCompletionService
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, bool jsonMode = false, CancellationToken ct = default);
}

/// <summary>
/// Extension point for future Retrieval-Augmented Generation over a Food Knowledge Base
/// (nutrition PDFs, recipe documents, Azure AI Search index, etc). The default implementation
/// is a no-op until a Food knowledge index is provisioned.
/// </summary>
public interface IFoodKnowledgeRetriever
{
    Task<List<string>> SearchRelevantChunksAsync(string query, int topK = 5, CancellationToken ct = default);
}

/// <summary>
/// Extension point for future pluggable Food AI Agents (e.g. a dedicated Recipe Agent,
/// Nutrition Agent, Meal-Planning Agent). Implementations can be registered via DI and
/// resolved without changing calling code.
/// </summary>
public interface IFoodAgent
{
    string Name { get; }
    Task<string> HandleAsync(string input, CancellationToken ct = default);
}

/// <summary>
/// Result of a Food module safety check (Prompt Shields and/or Content Safety).
/// </summary>
public record FoodSafetyCheckResult(bool IsSafe, bool InjectionDetected, string? Reason);

/// <summary>
/// Dedicated Responsible AI service for TalentMind NutriAI (Food module) backed by Azure AI
/// Content Safety (Prompt Shields for injection/jailbreak detection, Content Safety for harmful
/// content moderation). Fully separate from the Resume Assistant's <see cref="IResponsibleAiService"/>.
/// Applied to both user input (before calling the AI) and AI-generated output (before it is
/// persisted/returned) across chat, recipes, nutrition, and meal planning.
/// </summary>
public interface IFoodResponsibleAiService
{
    Task<FoodSafetyCheckResult> ValidateUserInputAsync(string text, CancellationToken ct = default);
    Task<FoodSafetyCheckResult> ValidateAiOutputAsync(string text, CancellationToken ct = default);
}

public interface IFoodChatService
{
    Task<FoodChatQueryResponse> QueryAsync(Guid userId, FoodChatQueryRequest request, CancellationToken ct = default);
    Task SubmitFeedbackAsync(FoodChatFeedbackRequest request, CancellationToken ct = default);
}

public interface IRecipeService
{
    Task<RecipeDto> GenerateAsync(Guid userId, RecipeGenerateRequest request, CancellationToken ct = default);
    Task<RecipeListResponse> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<RecipeDto?> GetByIdAsync(Guid userId, Guid recipeId, CancellationToken ct = default);
}

public interface INutritionService
{
    Task<NutritionAnalysisDto> AnalyzeAsync(Guid userId, NutritionAnalyzeRequest request, CancellationToken ct = default);
    Task<List<NutritionAnalysisDto>> GetHistoryAsync(Guid userId, CancellationToken ct = default);
}

public interface IMealPlannerService
{
    Task<MealPlanDto> GenerateAsync(Guid userId, MealPlanGenerateRequest request, CancellationToken ct = default);
    Task<MealPlanListResponse> GetAllAsync(Guid userId, CancellationToken ct = default);
}
