using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentMindAI.API.Extensions;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("query")]
    public async Task<ActionResult<ChatQueryResponse>> Query(ChatQueryRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _chatService.QueryAsync(userId, request, ct);
        return Ok(result);
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> Feedback(ChatFeedbackRequest request, CancellationToken ct)
    {
        await _chatService.SubmitFeedbackAsync(request, ct);
        return NoContent();
    }
}
