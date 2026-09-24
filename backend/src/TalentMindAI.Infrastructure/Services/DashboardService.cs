using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var totalResumes = await _dbContext.Resumes.CountAsync(ct);

        var skillCounts = (await _dbContext.Skills
            .GroupBy(s => s.Name)
            .Select(g => new { Skill = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(ct))
            .Select(x => new SkillCountDto(x.Skill, x.Count))
            .ToList();

        var analyses = await _dbContext.ResumeAnalyses.Select(a => a.YearsOfExperience).ToListAsync(ct);
        var buckets = new List<ExperienceBucketDto>
        {
            new("0-2 years", analyses.Count(y => y is >= 0 and < 2)),
            new("2-5 years", analyses.Count(y => y is >= 2 and < 5)),
            new("5-10 years", analyses.Count(y => y is >= 5 and < 10)),
            new("10+ years", analyses.Count(y => y >= 10))
        };

        return new DashboardStatsDto(totalResumes, skillCounts, buckets, skillCounts);
    }
}
