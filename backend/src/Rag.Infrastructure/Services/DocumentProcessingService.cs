using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Domain.Enums;
using Rag.Infrastructure.Persistence;
using Rag.Shared.Options;

namespace Rag.Infrastructure.Services;

public class DocumentProcessingService(
    ApplicationDbContext dbContext,
    ITextExtractionService extractionService,
    IChunkingService chunkingService,
    IEmbeddingService embeddingService,
    IOptions<ChunkingOptions> chunkOptions,
    ILogger<DocumentProcessingService> logger) : IDocumentProcessingService
{
    private readonly ChunkingOptions _chunkOptions = chunkOptions.Value;

    public async Task ProcessAsync(Guid documentId, string filePath, CancellationToken ct = default)
    {
        var document = await dbContext.Documents.FirstAsync(x => x.Id == documentId, ct);
        document.Status = DocumentStatus.Processing;
        await dbContext.SaveChangesAsync(ct);

        try
        {
            var ext = Path.GetExtension(filePath);
            var text = await extractionService.ExtractTextAsync(filePath, ext, ct);
            var chunks = chunkingService.ChunkText(text, _chunkOptions.DefaultChunkSize, _chunkOptions.DefaultOverlap);

            var chunkEntities = new List<DocumentChunk>();
            var index = 0;
            foreach (var chunk in chunks)
            {
                var embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Text, ct);
                chunkEntities.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    ChunkIndex = index++,
                    ChunkText = chunk.Text,
                    TokenCount = chunk.TokenCount,
                    EmbeddingJson = JsonSerializer.Serialize(embedding),
                    CreatedAt = DateTime.UtcNow
                });
            }

            dbContext.DocumentChunks.AddRange(chunkEntities);
            document.Status = DocumentStatus.Processed;
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Document processing failed for {DocumentId}", documentId);
            document.Status = DocumentStatus.Failed;
            document.ErrorMessage = ex.Message;
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
