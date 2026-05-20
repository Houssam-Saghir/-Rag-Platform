using Rag.Domain.Entities;

namespace Rag.Application.Interfaces;

public interface IAiChatService
{
    Task<string> AskWithContextAsync(string question, IReadOnlyCollection<DocumentChunk> chunks, IReadOnlyCollection<ChatMessage> history, CancellationToken ct = default);
}
