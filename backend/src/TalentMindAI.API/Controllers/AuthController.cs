using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly FeatureFlagsOptions _featureFlags;

    public AuthController(IAuthService authService, IOptions<FeatureFlagsOptions> featureFlags)
    {
        _authService = authService;
        _featureFlags = featureFlags.Value;
    }

    /// <summary>Exposes which authentication mode is currently active so the UI can adapt.</summary>
    [HttpGet("config")]
    public ActionResult<object> GetConfig()
    {
        return Ok(new
        {
            authMode = _featureFlags.AuthMode.ToString()
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        if (_featureFlags.AuthMode != AuthMode.Basic)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Basic authentication is currently disabled.");
        }

        var result = await _authService.RegisterAsync(request, ct);
        SetAuthCookie(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        if (_featureFlags.AuthMode != AuthMode.Basic)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Basic authentication is currently disabled.");
        }

        var result = await _authService.LoginAsync(request, ct);
        SetAuthCookie(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("tma_auth");
        return Ok();
    }

    private void SetAuthCookie(AuthResponse result)
    {
        Response.Cookies.Append("tma_auth", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = result.ExpiresAt
        });
    }
}
