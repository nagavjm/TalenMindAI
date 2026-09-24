using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentMindAI.API.Extensions;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.API.Controllers;

[ApiController]
[Route("api/resume")]
[Authorize]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _resumeService;

    public ResumeController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ResumeUploadResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _resumeService.UploadAsync(userId, file, ct);
        return Ok(result);
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ResumeAnalysisDto>> Analyze(ResumeAnalysisRequest request, CancellationToken ct)
    {
        var result = await _resumeService.AnalyzeAsync(request.ResumeId, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ResumeSummaryDto>>> GetAll(CancellationToken ct)
    {
        var userId = User.IsInRole("Admin") ? (Guid?)null : User.GetUserId();
        var result = await _resumeService.GetResumesAsync(userId, ct);
        return Ok(result);
    }

    [HttpGet("{resumeId:guid}")]
    public async Task<ActionResult<CandidateDetailsDto>> GetDetails(Guid resumeId, CancellationToken ct)
    {
        var result = await _resumeService.GetCandidateDetailsAsync(resumeId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
