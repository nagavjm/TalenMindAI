using TalentMindAI.Domain.Common;
using TalentMindAI.Domain.Enums;

namespace TalentMindAI.Domain.Entities;

/// <summary>
/// Chat history for the TalentMind NutriAI Food Assistant. Kept fully separate
/// from the Resume Assistant's <see cref="ChatHistory"/> table.
/// </summary>
public class FoodChatHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? RetrievedChunksJson { get; set; }
    public FeedbackRating Feedback { get; set; } = FeedbackRating.NotRated;
}
