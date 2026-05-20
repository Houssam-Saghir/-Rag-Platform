using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Rag.Application.Interfaces;
using Rag.Shared.Options;

namespace Rag.Infrastructure.Services;

public class EmbeddingService(HttpClient httpClient, IOptions<OpenAiOptions> options, ILogger<EmbeddingService> logger) : IEmbeddingService
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var payload = new { model = _options.EmbeddingModel, input = text };
        var response = await Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt))
            .ExecuteAsync(() =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "/v1/embeddings");
                req.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
                req.Content = JsonContent.Create(payload);
                return httpClient.SendAsync(req, ct);
            });

        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("OpenAI embedding response was empty");

        logger.LogDebug("Embedding generated with {Count} dimensions", doc.Data.First().Embedding.Length);
        return doc.Data.First().Embedding;
    }

    private sealed class EmbeddingResponse
    {
        public List<EmbeddingData> Data { get; set; } = [];
    }

    private sealed class EmbeddingData
    {
        public float[] Embedding { get; set; } = [];
    }
}
