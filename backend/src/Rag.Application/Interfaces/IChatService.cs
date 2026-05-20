using Rag.Application.DTOs;

namespace Rag.Application.Interfaces;

public interface IChatService
{
    Task<ChatSessionDto> CreateSessionAsync(Guid userId, Guid documentId, string? title, CancellationToken ct = default);
    Task<IReadOnlyCollection<ChatSessionDto>> GetSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<ChatMessageDto>> GetHistoryAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task<ChatMessageDto> AskQuestionAsync(Guid userId, Guid sessionId, AskQuestionRequest request, CancellationToken ct = default);
    Task DeleteSessionAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
}
