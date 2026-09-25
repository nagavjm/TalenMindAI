using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentMindAI.API.Extensions;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.API.Controllers;

[ApiController]
[Route("api/food/nutrition")]
[Authorize]
public class FoodNutritionController : ControllerBase
{
    private readonly INutritionService _nutritionService;

    public FoodNutritionController(INutritionService nutritionService)
    {
        _nutritionService = nutritionService;
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<NutritionAnalysisDto>>> GetHistory(CancellationToken ct)
    {
        var userId = User.GetUserId();
        return Ok(await _nutritionService.GetHistoryAsync(userId, ct));
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<NutritionAnalysisDto>> Analyze(NutritionAnalyzeRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _nutritionService.AnalyzeAsync(userId, request, ct);
        return Ok(result);
    }
}
