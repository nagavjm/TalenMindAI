using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentMindAI.API.Extensions;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.API.Controllers;

[ApiController]
[Route("api/food/meal-planner")]
[Authorize]
public class FoodMealPlannerController : ControllerBase
{
    private readonly IMealPlannerService _mealPlannerService;

    public FoodMealPlannerController(IMealPlannerService mealPlannerService)
    {
        _mealPlannerService = mealPlannerService;
    }

    [HttpGet]
    public async Task<ActionResult<MealPlanListResponse>> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        return Ok(await _mealPlannerService.GetAllAsync(userId, ct));
    }

    [HttpPost("generate")]
    public async Task<ActionResult<MealPlanDto>> Generate(MealPlanGenerateRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mealPlannerService.GenerateAsync(userId, request, ct);
        return Ok(result);
    }
}
