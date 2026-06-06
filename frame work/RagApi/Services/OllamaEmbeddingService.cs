using System.Net.Http.Json;
using RagApi.Models;

namespace RagApi.Services;

/// <summary>
/// Generates embeddings using the Ollama local API.
/// Calls <c>POST /api/embeddings</c> with the configured embedding model.
/// </summary>
public sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OllamaEmbeddingService> _logger;

    public OllamaEmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OllamaEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text for embedding cannot be empty.", nameof(text));

        var request = new OllamaEmbeddingRequest
        {
            Model = _model,
            Prompt = text
        };

        _logger.LogDebug("Generating embedding with model '{Model}' for text of length {Length}",
            _model, text.Length);

        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: cancellationToken);

        if (result?.Embedding is null || result.Embedding.Length == 0)
            throw new InvalidOperationException("Ollama returned an empty embedding vector.");

        _logger.LogDebug("Generated embedding with {Dimensions} dimensions", result.Embedding.Length);
        return result.Embedding;
    }
}
