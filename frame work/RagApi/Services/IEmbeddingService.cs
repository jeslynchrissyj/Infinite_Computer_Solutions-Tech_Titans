namespace RagApi.Services;

/// <summary>
/// Generates embedding vectors for text input.
/// Implement this interface for each LLM provider (Ollama, OpenAI, etc.).
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for the given text.
    /// </summary>
    /// <param name="text">The input text to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A float array representing the embedding vector.</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
