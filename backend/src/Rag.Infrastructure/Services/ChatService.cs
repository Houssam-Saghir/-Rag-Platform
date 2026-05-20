using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Rag.Application.DTOs;
using Rag.Application.Exceptions;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Domain.Enums;
using Rag.Infrastructure.Persistence;

namespace Rag.Infrastructure.Services;

public class ChatService(ApplicationDbContext dbContext, IVectorSearchService vectorSearchService, IAiChatService aiChatService) : IChatService
{
    public async Task<ChatSessionDto> CreateSessionAsync(Guid userId, Guid documentId, string? title, CancellationToken ct = default)
    {
        var docExists = await dbContext.Documents.AnyAsync(x => x.Id == documentId && x.UploadedByUserId == userId, ct);
        if (!docExists) throw new NotFoundException("Document not found");

        var session = new ChatSession
        {
            UserId = userId,
            DocumentId = documentId,
            Title = string.IsNullOrWhiteSpace(title) ? "New Chat" : title.Trim()
        };
        dbContext.ChatSessions.Add(session);
        await dbContext.SaveChangesAsync(ct);

        return new ChatSessionDto(session.Id, session.Title, session.CreatedAt, 0, session.DocumentId);
    }

    public async Task<IReadOnlyCollection<ChatSessionDto>> GetSessionsAsync(Guid userId, CancellationToken ct = default)
        => await dbContext.ChatSessions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ChatSessionDto(x.Id, x.Title, x.CreatedAt, x.Messages.Count, x.DocumentId))
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<ChatMessageDto>> GetHistoryAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
    {
        var session = await dbContext.ChatSessions
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found");

        return session.Messages.OrderBy(x => x.CreatedAt)
            .Select(m => new ChatMessageDto(m.Id, m.Role.ToString(), m.Content, m.CreatedAt,
                string.IsNullOrWhiteSpace(m.SourceChunkIdsJson) ? null : JsonSerializer.Deserialize<List<Guid>>(m.SourceChunkIdsJson)))
            .ToList();
    }

    public async Task<ChatMessageDto> AskQuestionAsync(Guid userId, Guid sessionId, AskQuestionRequest request, CancellationToken ct = default)
    {
        var session = await dbContext.ChatSessions
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found");

        if (session.DocumentId is null) throw new ValidationException("Session has no document");

        var userMsg = new ChatMessage { ChatSessionId = sessionId, Role = MessageRole.User, Content = request.Question };
        dbContext.ChatMessages.Add(userMsg);

        var relevant = await vectorSearchService.SearchAsync(session.DocumentId.Value, request.Question, 5, ct);
        var response = await aiChatService.AskWithContextAsync(request.Question, relevant.Select(x => x.Chunk).ToList(), session.Messages.ToList(), ct);

        var sourceIds = relevant.Select(x => x.Chunk.Id).ToList();
        var assistantMsg = new ChatMessage
        {
            ChatSessionId = sessionId,
            Role = MessageRole.Assistant,
            Content = response,
            SourceChunkIdsJson = JsonSerializer.Serialize(sourceIds)
        };

        dbContext.ChatMessages.Add(assistantMsg);
        await dbContext.SaveChangesAsync(ct);

        return new ChatMessageDto(assistantMsg.Id, assistantMsg.Role.ToString(), assistantMsg.Content, assistantMsg.CreatedAt, sourceIds);
    }

    public async Task DeleteSessionAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
    {
        var session = await dbContext.ChatSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found");

        dbContext.ChatSessions.Remove(session);
        await dbContext.SaveChangesAsync(ct);
    }
}
