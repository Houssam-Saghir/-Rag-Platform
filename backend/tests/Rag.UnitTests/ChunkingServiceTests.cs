using Rag.Infrastructure.Services;

namespace Rag.UnitTests;

public class ChunkingServiceTests
{
    [Fact]
    public void ChunkText_ShouldCreateChunks_WhenTextIsLong()
    {
        var service = new ChunkingService();
        var text = string.Join(". ", Enumerable.Range(1, 20).Select(i => $"Sentence {i} with multiple words"));

        var result = service.ChunkText(text, 20, 5);

        Assert.NotEmpty(result);
        Assert.All(result, item => Assert.False(string.IsNullOrWhiteSpace(item.Text)));
    }
}
