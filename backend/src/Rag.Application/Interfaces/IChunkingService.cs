namespace Rag.Application.Interfaces;

public interface IChunkingService
{
    IReadOnlyCollection<(string Text, int TokenCount)> ChunkText(string text, int chunkSize, int overlap);
}
