namespace RagApi.Services;

/// <summary>
/// Mock embedding service that generates dummy vectors for testing and offline cloud deployment.
/// </summary>
public sealed class MockEmbeddingService : IEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // Return a dummy float array of size 768 (nomic-embed-text size)
        var dummy = new float[768];
        if (text.Length > 0)
        {
            dummy[0] = 1.0f; // Set a value to ensure non-zero magnitudes
        }
        return Task.FromResult(dummy);
    }
}
