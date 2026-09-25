using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentMindAI.API.Extensions;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.API.Controllers;

[ApiController]
[Route("api/food/recipes")]
[Authorize]
public class FoodRecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public FoodRecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [HttpGet]
    public async Task<ActionResult<RecipeListResponse>> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        return Ok(await _recipeService.GetAllAsync(userId, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecipeDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var recipe = await _recipeService.GetByIdAsync(userId, id, ct);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<RecipeDto>> Generate(RecipeGenerateRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var recipe = await _recipeService.GenerateAsync(userId, request, ct);
        return Ok(recipe);
    }
}
