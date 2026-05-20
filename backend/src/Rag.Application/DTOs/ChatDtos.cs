namespace Rag.Application.DTOs;

public record CreateSessionRequest(Guid DocumentId, string? Title);
public record AskQuestionRequest(string Question, Guid DocumentId);
public record ChatMessageDto(Guid Id, string Role, string Content, DateTime CreatedAt, List<Guid>? SourceChunkIds);
public record ChatSessionDto(Guid Id, string Title, DateTime CreatedAt, int MessageCount, Guid? DocumentId);
