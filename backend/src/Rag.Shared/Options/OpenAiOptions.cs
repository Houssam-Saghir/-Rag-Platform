namespace Rag.Shared.Options;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
    public string ChatModel { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 2000;
    public float Temperature { get; set; } = 0.1f;
}
