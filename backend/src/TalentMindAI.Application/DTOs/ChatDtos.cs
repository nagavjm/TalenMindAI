namespace TalentMindAI.Application.DTOs;

public record ChatQueryRequest(string Question, Guid? ResumeId);

public record ChatQueryResponse(Guid ChatHistoryId, string Answer, List<string> SourceChunks);

public record ChatFeedbackRequest(Guid ChatHistoryId, bool IsHelpful);

public record InterviewQuestionRequest(Guid ResumeId);

public record InterviewQuestionDto(string Question, string Difficulty, string Skill);

public record InterviewQuestionResponse(Guid ResumeId, List<InterviewQuestionDto> Questions);
