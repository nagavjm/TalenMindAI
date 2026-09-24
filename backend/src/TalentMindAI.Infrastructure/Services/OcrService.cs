using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Options;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly AiVisionOptions _options;

    public OcrService(IOptions<AiVisionOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> ExtractTextFromImageAsync(Stream imageStream, CancellationToken ct = default)
    {
        var client = new ImageAnalysisClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));

        using var memory = new MemoryStream();
        await imageStream.CopyToAsync(memory, ct);
        memory.Position = 0;

        var result = await client.AnalyzeAsync(
            BinaryData.FromStream(memory),
            VisualFeatures.Read,
            cancellationToken: ct);

        var lines = result.Value.Read?.Blocks.SelectMany(b => b.Lines).Select(l => l.Text) ?? Enumerable.Empty<string>();
        return string.Join(Environment.NewLine, lines);
    }
}
