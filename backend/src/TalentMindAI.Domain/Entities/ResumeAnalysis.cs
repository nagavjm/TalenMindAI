using TalentMindAI.Domain.Common;

namespace TalentMindAI.Domain.Entities;

public class ResumeAnalysis : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Resume? Resume { get; set; }

    public string CandidateSummary { get; set; } = string.Empty;
    public string TechnicalSkillsJson { get; set; } = "[]";
    public string CertificationsJson { get; set; } = "[]";
    public decimal YearsOfExperience { get; set; }
    public string StrengthsJson { get; set; } = "[]";
    public string SuggestedRolesJson { get; set; } = "[]";
    public string? KeyPhrasesJson { get; set; }
    public string? NamedEntitiesJson { get; set; }
    public string RawModelResponse { get; set; } = string.Empty;
}
