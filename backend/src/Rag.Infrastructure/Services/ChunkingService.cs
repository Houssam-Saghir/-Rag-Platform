using System.Text.RegularExpressions;
using Rag.Application.Interfaces;

namespace Rag.Infrastructure.Services;

public class ChunkingService : IChunkingService
{
    public IReadOnlyCollection<(string Text, int TokenCount)> ChunkText(string text, int chunkSize, int overlap)
    {
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var chunks = new List<(string Text, int TokenCount)>();
        var current = new List<string>();
        var currentTokens = 0;

        foreach (var sentence in sentences)
        {
            var tokens = EstimateTokens(sentence);
            if (currentTokens + tokens > chunkSize && current.Count > 0)
            {
                chunks.Add((string.Join(" ", current), currentTokens));
                var overlapText = string.Join(" ", current).Split(' ').TakeLast(Math.Max(overlap, 0)).ToArray();
                current = overlapText.Length == 0 ? new List<string>() : [string.Join(" ", overlapText)];
                currentTokens = overlapText.Length;
            }

            current.Add(sentence);
            currentTokens += tokens;
        }

        if (current.Count > 0)
        {
            chunks.Add((string.Join(" ", current), currentTokens));
        }

        return chunks;
    }

    private static int EstimateTokens(string text) => Math.Max(1, text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
}
