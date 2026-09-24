using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Options;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

public class LanguageService : ILanguageService
{
    private readonly AiLanguageOptions _options;

    public LanguageService(IOptions<AiLanguageOptions> options)
    {
        _options = options.Value;
    }

    private TextAnalyticsClient Client => new(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));

    public async Task<List<string>> ExtractKeyPhrasesAsync(string text, CancellationToken ct = default)
    {
        var response = await Client.ExtractKeyPhrasesAsync(text, cancellationToken: ct);
        return response.Value.ToList();
    }

    public async Task<List<string>> RecognizeEntitiesAsync(string text, CancellationToken ct = default)
    {
        var response = await Client.RecognizeEntitiesAsync(text, cancellationToken: ct);
        return response.Value.Select(e => $"{e.Text} ({e.Category})").ToList();
    }

    public async Task<string> MaskPiiAsync(string text, CancellationToken ct = default)
    {
        var response = await Client.RecognizePiiEntitiesAsync(text, cancellationToken: ct);
        return response.Value.RedactedText;
    }
}
