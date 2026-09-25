using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentMindAI.API.Extensions;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.API.Controllers;

/// <summary>
/// TalentMind NutriAI (Food module) chat endpoint. Fully separate from the Resume Assistant's /api/chat.
/// </summary>
[ApiController]
[Route("api/food/chat")]
[Authorize]
public class FoodChatController : ControllerBase
{
    private readonly IFoodChatService _foodChatService;

    public FoodChatController(IFoodChatService foodChatService)
    {
        _foodChatService = foodChatService;
    }

    [HttpPost("query")]
    public async Task<ActionResult<FoodChatQueryResponse>> Query(FoodChatQueryRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _foodChatService.QueryAsync(userId, request, ct);
        return Ok(result);
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> Feedback(FoodChatFeedbackRequest request, CancellationToken ct)
    {
        await _foodChatService.SubmitFeedbackAsync(request, ct);
        return NoContent();
    }
}
