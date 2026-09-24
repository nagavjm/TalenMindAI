using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.Options;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

public class AiFoundryService : IAiFoundryService
{
    private readonly AiFoundryOptions _options;

    public AiFoundryService(IOptions<AiFoundryOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var client = new ChatCompletionsClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));

        var requestOptions = new ChatCompletionsOptions
        {
            Model = _options.DeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var response = await client.CompleteAsync(requestOptions, ct);
        return response.Value.Content ?? string.Empty;
    }
}
