using TalentMindAI.Domain.Common;

namespace TalentMindAI.Domain.Entities;

/// <summary>
/// A recipe generated or saved within the TalentMind NutriAI Food module.
/// </summary>
public class FoodRecipe : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IngredientsJson { get; set; } = "[]";
    public string Instructions { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public string DietaryTagsJson { get; set; } = "[]";
    public int? CaloriesPerServing { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;
    public bool GeneratedByAi { get; set; }
}
