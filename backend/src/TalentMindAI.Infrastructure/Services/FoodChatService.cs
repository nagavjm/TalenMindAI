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
    private readonly AppDbContext _dbContext;
    private readonly IFoodKnowledgeRetriever _knowledgeRetriever;
    private readonly IFoodAiCompletionService _foodAi;
    private readonly IResponsibleAiService _responsibleAi;
    private readonly IPromptLogService _promptLog;

    public FoodChatService(
        AppDbContext dbContext,
        IFoodKnowledgeRetriever knowledgeRetriever,
        IFoodAiCompletionService foodAi,
        IResponsibleAiService responsibleAi,
        IPromptLogService promptLog)
    {
        _dbContext = dbContext;
        _knowledgeRetriever = knowledgeRetriever;
        _foodAi = foodAi;
        _responsibleAi = responsibleAi;
        _promptLog = promptLog;
    }

    public async Task<FoodChatQueryResponse> QueryAsync(Guid userId, FoodChatQueryRequest request, CancellationToken ct = default)
    {
        if (_responsibleAi.DetectPromptInjection(request.Question))
        {
            throw new InvalidOperationException("Potential prompt injection detected in the request.");
        }

        var chunks = await _knowledgeRetriever.SearchRelevantChunksAsync(request.Question, 5, ct);
        var context = chunks.Count > 0 ? string.Join("\n---\n", chunks) : "No additional food knowledge base context is available yet.";

        const string systemPrompt = "You are TalentMind NutriAI, a friendly and knowledgeable food, nutrition, recipe, and meal-planning assistant. Use the provided context when relevant, and otherwise answer using general nutrition knowledge. Never provide medical diagnoses.";
        var userPrompt = $"Context:\n{context}\n\nQuestion: {request.Question}";

        var answer = await _foodAi.CompleteAsync(systemPrompt, userPrompt, jsonMode: false, ct);

        await _promptLog.LogAsync(userId, "FoodChat", request.Question, answer, containedPii: false, failedSafety: false, injectionDetected: false, latencyMs: 0, ct);

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

    public async Task SubmitFeedbackAsync(FoodChatFeedbackRequest request, CancellationToken ct = default)
    {
        var history = await _dbContext.FoodChatHistories.FirstOrDefaultAsync(c => c.Id == request.ChatHistoryId, ct)
            ?? throw new KeyNotFoundException("Food chat history not found.");

        history.Feedback = request.IsHelpful ? FeedbackRating.Helpful : FeedbackRating.NotHelpful;
        await _dbContext.SaveChangesAsync(ct);
    }
}
