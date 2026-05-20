namespace Rag.Shared.Options;

public class ChunkingOptions
{
    public const string SectionName = "Chunking";
    public int DefaultChunkSize { get; set; } = 500;
    public int DefaultOverlap { get; set; } = 50;
}
