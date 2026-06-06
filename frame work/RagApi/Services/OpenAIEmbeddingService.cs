using System.Net.Http.Json;
using RagApi.Models;

namespace RagApi.Services;

/// <summary>
/// Generates embeddings using the OpenAI API.
/// </summary>
public sealed class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OpenAIEmbeddingService> _logger;

    public OpenAIEmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAIEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text for embedding cannot be empty.", nameof(text));

        var request = new OpenAIEmbeddingRequest
        {
            Model = _model,
            Input = text
        };

        _logger.LogDebug("Generating OpenAI embedding with model '{Model}'", _model);

        var response = await _httpClient.PostAsJsonAsync("/v1/embeddings", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>(cancellationToken: cancellationToken);

        if (result?.Data == null || result.Data.Count == 0 || result.Data[0].Embedding.Length == 0)
            throw new InvalidOperationException("OpenAI returned an empty embedding vector.");

        _logger.LogDebug("Generated OpenAI embedding with {Dimensions} dimensions", result.Data[0].Embedding.Length);
        return result.Data[0].Embedding;
    }
}
