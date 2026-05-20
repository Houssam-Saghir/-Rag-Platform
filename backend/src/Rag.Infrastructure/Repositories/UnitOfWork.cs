using Rag.Domain.Entities;
using Rag.Infrastructure.Interfaces;
using Rag.Infrastructure.Persistence;

namespace Rag.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private IRepository<Document>? _documents;
    private IRepository<DocumentChunk>? _documentChunks;
    private IRepository<ChatSession>? _chatSessions;
    private IRepository<ChatMessage>? _chatMessages;
    private IRepository<User>? _users;
    private IRepository<Role>? _roles;
    private IRepository<RefreshToken>? _refreshTokens;

    public IRepository<Document> Documents => _documents ??= new Repository<Document>(dbContext);
    public IRepository<DocumentChunk> DocumentChunks => _documentChunks ??= new Repository<DocumentChunk>(dbContext);
    public IRepository<ChatSession> ChatSessions => _chatSessions ??= new Repository<ChatSession>(dbContext);
    public IRepository<ChatMessage> ChatMessages => _chatMessages ??= new Repository<ChatMessage>(dbContext);
    public IRepository<User> Users => _users ??= new Repository<User>(dbContext);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(dbContext);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(dbContext);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
    public void Dispose() => dbContext.Dispose();
}
