using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

/// <summary>
/// Dedicated Responsible AI service for TalentMind NutriAI (Food module). Calls Azure AI Content
/// Safety's Prompt Shields (text:shieldPrompt) for injection/jailbreak detection and Content Safety
/// (text:analyze) for harmful content moderation, via raw HTTP (no SDK dependency required).
/// Fully isolated from the Resume Assistant's regex-based <see cref="ResponsibleAiService"/>.
/// Fails open (treats content as safe) if the resource isn't configured yet, so the Food module
/// keeps working before FoodContentSafety:Endpoint/ApiKey are provisioned.
/// </summary>
public class FoodResponsibleAiService : IFoodResponsibleAiService
{
    private const int SeverityBlockThreshold = 4; // Azure Content Safety severities: 0,2,4,6 (0-7 scale grouped)

    // Shared static HttpClient (no Microsoft.Extensions.Http/IHttpClientFactory dependency required)
    // to avoid socket exhaustion, matching guidance for long-lived singleton HttpClient usage.
    // A short timeout is critical here: if the corporate network/firewall silently drops outbound
    // calls to the Content Safety endpoint (rather than actively refusing them), the default
    // 100s HttpClient timeout would make every chat/recipe/nutrition/meal-planner request hang
    // for 100s+ before failing open, which the browser/API would report as a timeout well before that.
    private static readonly HttpClient SharedHttpClient = new() { Timeout = TimeSpan.FromSeconds(8) };

    private readonly FoodContentSafetyOptions _options;
    private readonly ILogger<FoodResponsibleAiService> _logger;

    public FoodResponsibleAiService(IOptions<FoodContentSafetyOptions> options, ILogger<FoodResponsibleAiService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.Endpoint) &&
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !_options.Endpoint.Contains("<your-", StringComparison.OrdinalIgnoreCase);

    public async Task<FoodSafetyCheckResult> ValidateUserInputAsync(string text, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            return new FoodSafetyCheckResult(true, false, null);
        }

        var injection = await ShieldPromptAsync(text, ct);
        if (injection)
        {
            return new FoodSafetyCheckResult(false, true, "Potential prompt injection/jailbreak attempt detected.");
        }

        return await AnalyzeContentAsync(text, ct);
    }

    public async Task<FoodSafetyCheckResult> ValidateAiOutputAsync(string text, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            return new FoodSafetyCheckResult(true, false, null);
        }

        return await AnalyzeContentAsync(text, ct);
    }

    private async Task<bool> ShieldPromptAsync(string text, CancellationToken ct)
    {
        try
        {
            var url = $"{_options.Endpoint.TrimEnd('/')}/contentsafety/text:shieldPrompt?api-version=2024-09-01";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _options.ApiKey);
            request.Content = JsonContent.Create(new { userPrompt = text, documents = Array.Empty<string>() });

            using var response = await SharedHttpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Food Prompt Shields call failed with status {Status}", response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<ShieldPromptResponse>(cancellationToken: ct);
            return result?.UserPromptAnalysis?.AttackDetected ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Food Prompt Shields call threw an exception; failing open.");
            return false;
        }
    }

    private async Task<FoodSafetyCheckResult> AnalyzeContentAsync(string text, CancellationToken ct)
    {
        try
        {
            var url = $"{_options.Endpoint.TrimEnd('/')}/contentsafety/text:analyze?api-version=2023-10-01";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _options.ApiKey);
            request.Content = JsonContent.Create(new { text });

            using var response = await SharedHttpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Food Content Safety call failed with status {Status}", response.StatusCode);
                return new FoodSafetyCheckResult(true, false, null);
            }

            var result = await response.Content.ReadFromJsonAsync<AnalyzeTextResponse>(cancellationToken: ct);
            var flagged = result?.CategoriesAnalysis?.FirstOrDefault(c => c.Severity >= SeverityBlockThreshold);
            if (flagged is not null)
            {
                return new FoodSafetyCheckResult(false, false, $"Content flagged as unsafe (category: {flagged.Category}, severity: {flagged.Severity}).");
            }

            return new FoodSafetyCheckResult(true, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Food Content Safety call threw an exception; failing open.");
            return new FoodSafetyCheckResult(true, false, null);
        }
    }

    private sealed class ShieldPromptResponse
    {
        [JsonPropertyName("userPromptAnalysis")]
        public UserPromptAnalysisResult? UserPromptAnalysis { get; set; }
    }

    private sealed class UserPromptAnalysisResult
    {
        [JsonPropertyName("attackDetected")]
        public bool AttackDetected { get; set; }
    }

    private sealed class AnalyzeTextResponse
    {
        [JsonPropertyName("categoriesAnalysis")]
        public List<CategoryAnalysis>? CategoriesAnalysis { get; set; }
    }

    private sealed class CategoryAnalysis
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("severity")]
        public int Severity { get; set; }
    }
}
