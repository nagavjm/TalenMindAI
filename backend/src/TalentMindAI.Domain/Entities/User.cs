using TalentMindAI.Domain.Common;
using TalentMindAI.Domain.Enums;

namespace TalentMindAI.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();

    // TalentMind NutriAI (Food module) navigation collections - kept separate from Resume Assistant data.
    public ICollection<FoodChatHistory> FoodChatHistories { get; set; } = new List<FoodChatHistory>();
    public ICollection<FoodRecipe> FoodRecipes { get; set; } = new List<FoodRecipe>();
    public ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
    public ICollection<NutritionAnalysis> NutritionAnalyses { get; set; } = new List<NutritionAnalysis>();
}
