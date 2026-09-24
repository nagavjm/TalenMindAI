using TalentMindAI.Application.DTOs;

namespace TalentMindAI.Application.Interfaces;

public interface IChatService
{
    Task<ChatQueryResponse> QueryAsync(Guid userId, ChatQueryRequest request, CancellationToken ct = default);
    Task SubmitFeedbackAsync(ChatFeedbackRequest request, CancellationToken ct = default);
}

public interface IInterviewService
{
    Task<InterviewQuestionResponse> GenerateAsync(InterviewQuestionRequest request, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}
