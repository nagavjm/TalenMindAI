using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
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

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, bool jsonMode = false, CancellationToken ct = default)
    {
        var azureClient = new AzureOpenAIClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));
        var chatClient = azureClient.GetChatClient(_options.DeploymentName);

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var options = new ChatCompletionOptions();
        if (jsonMode)
        {
            options.ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat();
        }

        var response = await chatClient.CompleteChatAsync(messages, options, ct);
        return response.Value.Content.Count > 0 ? response.Value.Content[0].Text : string.Empty;
    }
}
