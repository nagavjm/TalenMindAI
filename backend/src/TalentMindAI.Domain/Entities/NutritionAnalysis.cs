using TalentMindAI.Domain.Common;

namespace TalentMindAI.Domain.Entities;

/// <summary>
/// Nutrition breakdown for a food item/description within the TalentMind NutriAI Food module.
/// </summary>
public class NutritionAnalysis : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string FoodDescription { get; set; } = string.Empty;
    public double? Calories { get; set; }
    public double? ProteinGrams { get; set; }
    public double? CarbsGrams { get; set; }
    public double? FatGrams { get; set; }
    public string AnalysisJson { get; set; } = "{}";
}
