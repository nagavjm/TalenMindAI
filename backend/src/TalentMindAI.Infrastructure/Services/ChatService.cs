using Microsoft.EntityFrameworkCore;
using TalentMindAI.Application.DTOs;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Domain.Entities;
using TalentMindAI.Domain.Enums;
using TalentMindAI.Infrastructure.Data;
using System.Text.Json;

namespace TalentMindAI.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _dbContext;
    private readonly ISearchIndexService _searchIndex;
    private readonly IAiFoundryService _aiFoundry;
    private readonly IResponsibleAiService _responsibleAi;
    private readonly IPromptLogService _promptLog;

    public ChatService(AppDbContext dbContext, ISearchIndexService searchIndex, IAiFoundryService aiFoundry, IResponsibleAiService responsibleAi, IPromptLogService promptLog)
    {
        _dbContext = dbContext;
        _searchIndex = searchIndex;
        _aiFoundry = aiFoundry;
        _responsibleAi = responsibleAi;
        _promptLog = promptLog;
    }

    public async Task<ChatQueryResponse> QueryAsync(Guid userId, ChatQueryRequest request, CancellationToken ct = default)
    {
        if (_responsibleAi.DetectPromptInjection(request.Question))
        {
            throw new InvalidOperationException("Potential prompt injection detected in the request.");
        }

        var chunks = await _searchIndex.SearchRelevantChunksAsync(request.Question, 5, ct);
        var context = string.Join("\n---\n", chunks);

        const string systemPrompt = "You are a grounded RAG assistant for a resume intelligence platform. Only answer using the provided context. If the answer is not in the context, say you don't know.";
        var userPrompt = $"Context:\n{context}\n\nQuestion: {request.Question}";

        var answer = await _aiFoundry.CompleteAsync(systemPrompt, userPrompt, ct);

        await _promptLog.LogAsync(userId, "Chat", request.Question, answer, containedPii: false, failedSafety: false, injectionDetected: false, latencyMs: 0, ct);

        var history = new ChatHistory
        {
            UserId = userId,
            ResumeId = request.ResumeId,
            Question = request.Question,
            Answer = answer,
            RetrievedChunksJson = JsonSerializer.Serialize(chunks)
        };

        _dbContext.ChatHistories.Add(history);
        await _dbContext.SaveChangesAsync(ct);

        return new ChatQueryResponse(history.Id, answer, chunks);
    }

    public async Task SubmitFeedbackAsync(ChatFeedbackRequest request, CancellationToken ct = default)
    {
        var history = await _dbContext.ChatHistories.FirstOrDefaultAsync(c => c.Id == request.ChatHistoryId, ct)
            ?? throw new KeyNotFoundException("Chat history not found.");

        history.Feedback = request.IsHelpful ? FeedbackRating.Helpful : FeedbackRating.NotHelpful;
        await _dbContext.SaveChangesAsync(ct);
    }
}
