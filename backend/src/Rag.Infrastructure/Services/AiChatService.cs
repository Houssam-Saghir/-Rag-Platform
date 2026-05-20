using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Shared.Options;

namespace Rag.Infrastructure.Services;

public class AiChatService(HttpClient httpClient, IOptions<OpenAiOptions> options) : IAiChatService
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<string> AskWithContextAsync(string question, IReadOnlyCollection<DocumentChunk> chunks, IReadOnlyCollection<ChatMessage> history, CancellationToken ct = default)
    {
        var context = string.Join("\n\n", chunks.Select(c => c.ChunkText));
        var messages = new List<object>
        {
            new { role = "system", content = "Answer ONLY using the provided context. If not found, say: 'I could not find information about this in the provided document.'" },
            new { role = "system", content = $"Context:\n{context}" }
        };

        messages.AddRange(history.OrderBy(x => x.CreatedAt).Select(h => new { role = h.Role.ToString().ToLowerInvariant(), content = h.Content }));
        messages.Add(new { role = "user", content = question });

        var payload = new
        {
            model = _options.ChatModel,
            messages,
            max_tokens = _options.MaxTokens,
            temperature = _options.Temperature
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
        req.Content = JsonContent.Create(payload);

        var response = await httpClient.SendAsync(req, ct);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var content = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        return string.IsNullOrWhiteSpace(content)
            ? "I could not find information about this in the provided document."
            : content;
    }
}
