using Rag.Domain.Entities;

namespace Rag.Infrastructure.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Document> Documents { get; }
    IRepository<DocumentChunk> DocumentChunks { get; }
    IRepository<ChatSession> ChatSessions { get; }
    IRepository<ChatMessage> ChatMessages { get; }
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
