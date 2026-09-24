namespace TalentMindAI.Application.DTOs;

public record SkillCountDto(string Skill, int Count);

public record ExperienceBucketDto(string Range, int Count);

public record DashboardStatsDto(
    int TotalResumes,
    List<SkillCountDto> TopSkills,
    List<ExperienceBucketDto> ExperienceDistribution,
    List<SkillCountDto> MostPopularTechnologies);
