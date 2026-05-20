using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rag.API.Extensions;
using Rag.Application.DTOs;
using Rag.Application.Interfaces;

namespace Rag.API.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController(IChatService chatService) : ControllerBase
{
    [HttpPost("sessions")]
    public async Task<ActionResult<ChatSessionDto>> CreateSession(CreateSessionRequest request, CancellationToken ct)
        => Ok(await chatService.CreateSessionAsync(User.GetUserId(), request.DocumentId, request.Title, ct));

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyCollection<ChatSessionDto>>> GetSessions(CancellationToken ct)
        => Ok(await chatService.GetSessionsAsync(User.GetUserId(), ct));

    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyCollection<ChatMessageDto>>> GetMessages(Guid sessionId, CancellationToken ct)
        => Ok(await chatService.GetHistoryAsync(User.GetUserId(), sessionId, ct));

    [HttpPost("sessions/{sessionId:guid}/ask")]
    public async Task<ActionResult<ChatMessageDto>> Ask(Guid sessionId, AskQuestionRequest request, CancellationToken ct)
        => Ok(await chatService.AskQuestionAsync(User.GetUserId(), sessionId, request, ct));

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> Delete(Guid sessionId, CancellationToken ct)
    {
        await chatService.DeleteSessionAsync(User.GetUserId(), sessionId, ct);
        return NoContent();
    }
}
