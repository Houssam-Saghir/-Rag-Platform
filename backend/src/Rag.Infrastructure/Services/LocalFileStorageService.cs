using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Rag.Application.Exceptions;
using Rag.Application.Interfaces;
using Rag.Shared.Options;

namespace Rag.Infrastructure.Services;

public class LocalFileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(Guid userId, Guid documentId, IFormFile file, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
            throw new ValidationException($"Unsupported extension: {extension}");

        var maxBytes = _options.MaxFileSizeMB * 1024L * 1024L;
        if (file.Length <= 0 || file.Length > maxBytes)
            throw new ValidationException($"File size exceeds {_options.MaxFileSizeMB}MB limit");

        var folder = Path.Combine(_options.UploadPath, userId.ToString(), documentId.ToString());
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, Path.GetFileName(file.FileName));

        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream, ct);
        return path;
    }
}
