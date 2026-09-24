using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.API.Controllers;

[ApiController]
[Route("api/interview")]
[Authorize]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _interviewService;

    public InterviewController(IInterviewService interviewService)
    {
        _interviewService = interviewService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<InterviewQuestionResponse>> Generate(InterviewQuestionRequest request, CancellationToken ct)
    {
        var result = await _interviewService.GenerateAsync(request, ct);
        return Ok(result);
    }
}
