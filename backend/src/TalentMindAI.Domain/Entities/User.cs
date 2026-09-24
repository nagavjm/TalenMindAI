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
}
