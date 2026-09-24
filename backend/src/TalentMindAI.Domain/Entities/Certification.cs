using TalentMindAI.Domain.Common;

namespace TalentMindAI.Domain.Entities;

public class Certification : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Resume? Resume { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? IssuingBody { get; set; }
    public DateTime? IssuedDate { get; set; }
}
