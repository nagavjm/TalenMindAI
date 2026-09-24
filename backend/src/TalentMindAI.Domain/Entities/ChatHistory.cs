using TalentMindAI.Domain.Common;
using TalentMindAI.Domain.Enums;

namespace TalentMindAI.Domain.Entities;

public class ChatHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid? ResumeId { get; set; }
    public Resume? Resume { get; set; }

    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? RetrievedChunksJson { get; set; }
    public FeedbackRating Feedback { get; set; } = FeedbackRating.NotRated;
}
