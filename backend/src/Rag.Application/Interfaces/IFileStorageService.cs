using Microsoft.AspNetCore.Http;

namespace Rag.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Guid userId, Guid documentId, IFormFile file, CancellationToken ct = default);
}
