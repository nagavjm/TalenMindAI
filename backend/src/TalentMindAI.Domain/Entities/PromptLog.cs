using TalentMindAI.Domain.Common;

namespace TalentMindAI.Domain.Entities;

public class PromptLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string PromptType { get; set; } = string.Empty; // Analysis, Chat, Interview
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public bool ContainedPii { get; set; }
    public bool FailedContentSafety { get; set; }
    public bool PromptInjectionDetected { get; set; }
    public int LatencyMs { get; set; }
}
