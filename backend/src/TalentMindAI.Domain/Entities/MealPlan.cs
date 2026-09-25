using TalentMindAI.Domain.Common;

namespace TalentMindAI.Domain.Entities;

/// <summary>
/// A meal plan generated within the TalentMind NutriAI Food module.
/// </summary>
public class MealPlan : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PlanJson { get; set; } = "{}";
    public string? Notes { get; set; }
    public bool GeneratedByAi { get; set; }
}
