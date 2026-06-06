using System.Collections.Concurrent;
using System.Numerics;
using RagApi.Models;

namespace RagApi.Services;

/// <summary>
/// Thread-safe, in-memory vector store that stores document chunks and their embeddings.
/// Supports cosine-similarity search to retrieve the most relevant chunks for a query.
/// </summary>
public sealed class VectorStoreService
{
    private readonly ConcurrentBag<DocumentChunk> _chunks = new();
    private readonly ILogger<VectorStoreService> _logger;

    public VectorStoreService(ILogger<VectorStoreService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the total number of chunks currently stored.
    /// </summary>
    public int Count => _chunks.Count;

    /// <summary>
    /// Adds a document chunk with its precomputed embedding to the store.
    /// </summary>
    /// <param name="chunk">The document chunk (must have a non-null <see cref="DocumentChunk.Embedding"/>).</param>
    public void AddChunk(DocumentChunk chunk)
    {
        if (chunk.Embedding is null || chunk.Embedding.Length == 0)
            throw new ArgumentException("Cannot add a chunk without an embedding.", nameof(chunk));

        _chunks.Add(chunk);
    }

    /// <summary>
    /// Adds multiple document chunks to the store at once.
    /// </summary>
    public void AddChunks(IEnumerable<DocumentChunk> chunks)
    {
        foreach (var chunk in chunks)
            AddChunk(chunk);
    }

    /// <summary>
    /// Performs a cosine-similarity search against all stored chunks and returns the top-K most similar.
    /// </summary>
    /// <param name="queryEmbedding">The embedding vector of the user's query.</param>
    /// <param name="topK">The number of results to return.</param>
    /// <returns>The top-K chunks sorted by descending similarity.</returns>
    public List<(DocumentChunk Chunk, double Score)> Search(float[] queryEmbedding, int topK)
    {
        if (_chunks.IsEmpty)
        {
            _logger.LogWarning("Vector store is empty — no results to search.");
            return new List<(DocumentChunk, double)>();
        }

        _logger.LogDebug("Searching {Count} chunk(s) for top-{K} results", _chunks.Count, topK);

        var results = _chunks
            .Where(c => c.Embedding is not null)
            .Select(c => (Chunk: c, Score: CosineSimilarity(queryEmbedding, c.Embedding!)))
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .ToList();

        if (results.Count > 0)
            _logger.LogDebug("Top result score: {Score:F4} from '{Source}'",
                results[0].Score, results[0].Chunk.Source);

        return results;
    }

    /// <summary>
    /// Clears all stored chunks and embeddings.
    /// </summary>
    public void Clear()
    {
        _chunks.Clear();
        _logger.LogInformation("Vector store cleared.");
    }

    /// <summary>
    /// Computes the cosine similarity between two vectors.
    /// Uses SIMD-accelerated operations via <see cref="Vector{T}"/> when possible.
    /// </summary>
    /// <remarks>
    /// Cosine similarity = (A · B) / (||A|| × ||B||)
    /// Returns a value between -1 and 1, where 1 means identical direction.
    /// </remarks>
    private static double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException(
                $"Vector dimensions do not match: {vectorA.Length} vs {vectorB.Length}.");

        var dotProduct = 0.0;
        var magnitudeA = 0.0;
        var magnitudeB = 0.0;

        // Use SIMD-friendly loop
        var simdLength = Vector<float>.Count;
        var i = 0;

        // SIMD path: process vectors in SIMD-width strides
        if (Vector.IsHardwareAccelerated && vectorA.Length >= simdLength)
        {
            var vDot = Vector<float>.Zero;
            var vMagA = Vector<float>.Zero;
            var vMagB = Vector<float>.Zero;

            for (; i <= vectorA.Length - simdLength; i += simdLength)
            {
                var va = new Vector<float>(vectorA, i);
                var vb = new Vector<float>(vectorB, i);
                vDot += va * vb;
                vMagA += va * va;
                vMagB += vb * vb;
            }

            // Sum the SIMD lanes
            for (var j = 0; j < simdLength; j++)
            {
                dotProduct += vDot[j];
                magnitudeA += vMagA[j];
                magnitudeB += vMagB[j];
            }
        }

        // Scalar tail: process remaining elements
        for (; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        var magnitude = Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB);
        return magnitude == 0 ? 0 : dotProduct / magnitude;
    }
}
