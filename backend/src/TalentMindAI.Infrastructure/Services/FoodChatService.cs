using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Domain.Enums;
using TalentMindAI.Infrastructure.Data;

namespace TalentMindAI.Infrastructure.Services;

/// <summary>
/// Chat service for TalentMind NutriAI (Food module). Uses dedicated Food AI completion
/// and knowledge retrieval services - never the Resume Assistant's agents/models.
/// </summary>
public class FoodChatService : IFoodChatService
{
    private static readonly System.Text.RegularExpressions.Regex ImageIntentRegex = new(
        @"\b(image|picture|photo|photograph|drawing|draw|illustration|illustrate)\b",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;
    private readonly IFoodKnowledgeRetriever _knowledgeRetriever;
    private readonly IFoodAiCompletionService _foodAi;
    private readonly IFoodImageService _foodImage;
    private readonly IFoodResponsibleAiService _responsibleAi;
    private readonly IPromptLogService _promptLog;

    public FoodChatService(
        AppDbContext dbContext,
        IFoodKnowledgeRetriever knowledgeRetriever,
        IFoodAiCompletionService foodAi,
        IFoodImageService foodImage,
        IFoodResponsibleAiService responsibleAi,
        IPromptLogService promptLog)
    {
        _dbContext = dbContext;
        _knowledgeRetriever = knowledgeRetriever;
        _foodAi = foodAi;
        _foodImage = foodImage;
        _responsibleAi = responsibleAi;
        _promptLog = promptLog;
    }

    public async Task<FoodChatQueryResponse> QueryAsync(Guid userId, FoodChatQueryRequest request, CancellationToken ct = default)
    {
        var inputCheck = await _responsibleAi.ValidateUserInputAsync(request.Question, ct);
        if (!inputCheck.IsSafe)
        {
            await _promptLog.LogAsync(userId, "FoodChat", request.Question, string.Empty, containedPii: false, failedSafety: true, injectionDetected: inputCheck.InjectionDetected, latencyMs: 0, ct);
            throw new InvalidOperationException(inputCheck.Reason ?? "Potential prompt injection or unsafe content detected in the request.");
        }

        if (ImageIntentRegex.IsMatch(request.Question))
        {
            return await GenerateImageResponseAsync(userId, request, ct);
        }

        var chunks = await _knowledgeRetriever.SearchRelevantChunksAsync(request.Question, 5, ct);
        var context = chunks.Count > 0 ? string.Join("\n---\n", chunks) : "No additional food knowledge base context is available yet.";

        const string systemPrompt = "You are TalentMind NutriAI, a friendly and knowledgeable food, nutrition, recipe, and meal-planning assistant. Use the provided context when relevant, and otherwise answer using general nutrition knowledge. Never provide medical diagnoses.";
        var userPrompt = $"Context:\n{context}\n\nQuestion: {request.Question}";

        var answer = await _foodAi.CompleteAsync(systemPrompt, userPrompt, jsonMode: false, ct);

        var outputCheck = await _responsibleAi.ValidateAiOutputAsync(answer, ct);
        if (!outputCheck.IsSafe)
        {
            await _promptLog.LogAsync(userId, "FoodChat", request.Question, answer, containedPii: false, failedSafety: true, injectionDetected: false, latencyMs: 0, ct);
            answer = "Sorry, I can't provide that response. Please rephrase your question.";
        }
        else
        {
            await _promptLog.LogAsync(userId, "FoodChat", request.Question, answer, containedPii: false, failedSafety: false, injectionDetected: false, latencyMs: 0, ct);
        }

        var history = new FoodChatHistory
        {
            UserId = userId,
            Question = request.Question,
            Answer = answer,
            RetrievedChunksJson = JsonSerializer.Serialize(chunks)
        };

        _dbContext.FoodChatHistories.Add(history);
        await _dbContext.SaveChangesAsync(ct);

        return new FoodChatQueryResponse(history.Id, answer, chunks);
    }

    private async Task<FoodChatQueryResponse> GenerateImageResponseAsync(Guid userId, FoodChatQueryRequest request, CancellationToken ct)
    {
        var chunks = await _knowledgeRetriever.SearchRelevantChunksAsync(request.Question, 5, ct);
        var context = chunks.Count > 0 ? string.Join("\n---\n", chunks) : "No additional food knowledge base context is available yet.";

        const string systemPrompt = "You are TalentMind NutriAI, a friendly and knowledgeable food, nutrition, recipe, and meal-planning assistant. Use the provided context when relevant, and otherwise answer using general nutrition knowledge. Never provide medical diagnoses. " +
            "An image matching the user's request is being generated separately and will be displayed alongside your answer. Do NOT say you cannot show, generate, or share images or pictures - simply answer the question naturally (e.g. describe or discuss what is shown) as if the image will accompany your response.";
        var userPrompt = $"Context:\n{context}\n\nQuestion: {request.Question}";

        var textTask = _foodAi.CompleteAsync(systemPrompt, userPrompt, jsonMode: false, ct);
        var imageTask = _foodImage.GenerateImageAsync(request.Question, ct);

        string answer;
        string imageUrl;
        try
        {
            await Task.WhenAll(textTask, imageTask);
        }
        catch (Exception)
        {
            // Individual task exceptions are inspected below; WhenAll only surfaces the first.
        }

        if (textTask.IsCompletedSuccessfully)
        {
            answer = textTask.Result;
            var outputCheck = await _responsibleAi.ValidateAiOutputAsync(answer, ct);
            if (!outputCheck.IsSafe)
            {
                answer = "Sorry, I can't provide that response. Please rephrase your question.";
            }
        }
        else
        {
            answer = string.Empty;
        }

        imageUrl = imageTask.IsCompletedSuccessfully ? imageTask.Result : string.Empty;

        if (string.IsNullOrWhiteSpace(answer) && string.IsNullOrWhiteSpace(imageUrl))
        {
            answer = "Sorry, I couldn't generate a response right now. Please try again later.";
        }
        else if (string.IsNullOrWhiteSpace(answer))
        {
            answer = "Here's an image based on your request.";
        }
        else if (string.IsNullOrWhiteSpace(imageUrl))
        {
            answer += "\n\n(Sorry, I couldn't generate an image right now. Please try again later.)";
        }

        await _promptLog.LogAsync(userId, "FoodChatImage", request.Question, answer, containedPii: false, failedSafety: false, injectionDetected: false, latencyMs: 0, ct);

        var history = new FoodChatHistory
        {
            UserId = userId,
            Question = request.Question,
            Answer = answer,
            RetrievedChunksJson = JsonSerializer.Serialize(chunks)
        };

        _dbContext.FoodChatHistories.Add(history);
        await _dbContext.SaveChangesAsync(ct);

        return new FoodChatQueryResponse(history.Id, answer, chunks, string.IsNullOrEmpty(imageUrl) ? null : imageUrl);
    }

    public async Task SubmitFeedbackAsync(FoodChatFeedbackRequest request, CancellationToken ct = default)
    {
        var history = await _dbContext.FoodChatHistories.FirstOrDefaultAsync(c => c.Id == request.ChatHistoryId, ct)
            ?? throw new KeyNotFoundException("Food chat history not found.");

        history.Feedback = request.IsHelpful ? FeedbackRating.Helpful : FeedbackRating.NotHelpful;
        await _dbContext.SaveChangesAsync(ct);
    }
}
