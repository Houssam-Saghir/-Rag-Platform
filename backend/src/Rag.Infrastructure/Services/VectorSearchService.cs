using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Infrastructure.Persistence;

namespace Rag.Infrastructure.Services;

public class VectorSearchService(ApplicationDbContext dbContext, IEmbeddingService embeddingService) : IVectorSearchService
{
    public async Task<IReadOnlyCollection<(DocumentChunk Chunk, float Score)>> SearchAsync(Guid documentId, string question, int topK, CancellationToken ct = default)
    {
        var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(question, ct);
        var chunks = await dbContext.DocumentChunks.Where(x => x.DocumentId == documentId).ToListAsync(ct);
        return chunks
            .Select(chunk =>
            {
                var emb = JsonSerializer.Deserialize<float[]>(chunk.EmbeddingJson) ?? [];
                var score = CosineSimilarity(queryEmbedding, emb);
                return (chunk, score);
            })
            .OrderByDescending(x => x.score)
            .Take(topK)
            .Select(x => (x.chunk, x.score))
            .ToList();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length == 0 || b.Length == 0 || a.Length != b.Length) return 0;

        float dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        var denom = MathF.Sqrt(magA) * MathF.Sqrt(magB);
        return denom == 0 ? 0 : dot / denom;
    }
}
