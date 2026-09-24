using TalentMindAI.Domain.Common;
using TalentMindAI.Domain.Enums;

namespace TalentMindAI.Domain.Entities;

public class Resume : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string CandidateName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public ResumeFileType FileType { get; set; }
    public string BlobUri { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? ExtractedText { get; set; }
    public ResumeStatus Status { get; set; } = ResumeStatus.Uploaded;
    public string? SearchIndexId { get; set; }

    public ResumeAnalysis? Analysis { get; set; }
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
}
