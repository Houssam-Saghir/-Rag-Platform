using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rag.API.Extensions;
using Rag.Application.DTOs;
using Rag.Application.Interfaces;

namespace Rag.API.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public class DocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<ActionResult<UploadDocumentResponse>> Upload(IFormFile file, CancellationToken ct)
        => Ok(await documentService.UploadAsync(User.GetUserId(), file, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DocumentDto>>> Get(CancellationToken ct)
        => Ok(await documentService.GetMyDocumentsAsync(User.GetUserId(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await documentService.GetByIdAsync(User.GetUserId(), id, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await documentService.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
