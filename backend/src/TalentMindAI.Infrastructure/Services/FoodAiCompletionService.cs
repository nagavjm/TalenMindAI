using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

/// <summary>
/// AI completion service dedicated to TalentMind NutriAI (Food module). Uses its own
/// <see cref="FoodAIOptions"/> configuration/deployment - completely separate from the
/// Resume Assistant's AI Foundry configuration.
/// </summary>
public class FoodAiCompletionService : IFoodAiCompletionService
{
    private readonly FoodAIOptions _options;

    public FoodAiCompletionService(IOptions<FoodAIOptions> options)
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
