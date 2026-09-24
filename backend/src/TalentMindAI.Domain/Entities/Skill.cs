using TalentMindAI.Domain.Common;

namespace TalentMindAI.Domain.Entities;

public class Skill : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Resume? Resume { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public double? ProficiencyScore { get; set; }
}
