namespace TalentMindAI.Application.DTOs;

public record ResumeUploadResponse(Guid ResumeId, string FileName, string Status);

public record ResumeSummaryDto(
    Guid Id,
    string CandidateName,
    string FileName,
    string Status,
    DateTime CreatedAt);

public record ResumeAnalysisRequest(Guid ResumeId);

public record ResumeAnalysisDto(
    Guid ResumeId,
    string CandidateSummary,
    List<string> TechnicalSkills,
    List<string> Certifications,
    decimal YearsOfExperience,
    List<string> Strengths,
    List<string> SuggestedRoles,
    List<string>? KeyPhrases,
    List<string>? NamedEntities);

public record CandidateDetailsDto(
    Guid ResumeId,
    string CandidateName,
    string FileName,
    string Status,
    ResumeAnalysisDto? Analysis);
