using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Images;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

/// <summary>
/// Image-generation service dedicated to TalentMind NutriAI (Food module). Uses a DALL-E
/// deployment on the same Azure OpenAI resource as <see cref="FoodAiCompletionService"/>
/// (configured via <see cref="FoodAIOptions.ImageDeploymentName"/>) - completely separate
/// from the Resume Assistant.
/// </summary>
public class FoodImageService : IFoodImageService
{
    private readonly FoodAIOptions _options;

    public FoodImageService(IOptions<FoodAIOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> GenerateImageAsync(string prompt, CancellationToken ct = default)
    {
        var azureClient = new AzureOpenAIClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));
        var imageClient = azureClient.GetImageClient(_options.ImageDeploymentName);

        var imageOptions = new ImageGenerationOptions
        {
            Size = GeneratedImageSize.W1024xH1024
        };

        var result = await imageClient.GenerateImageAsync(prompt, imageOptions, ct);
        var generatedImage = result.Value;

        if (generatedImage.ImageUri is not null)
        {
            return generatedImage.ImageUri.ToString();
        }

        if (generatedImage.ImageBytes is not null)
        {
            return $"data:image/png;base64,{Convert.ToBase64String(generatedImage.ImageBytes.ToArray())}";
        }

        return string.Empty;
    }
}
