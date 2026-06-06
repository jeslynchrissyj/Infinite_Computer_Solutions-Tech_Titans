using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RagApi.Models;
using RagApi.Services;

namespace RagApi.Tests;

public class VectorStoreServiceTests
{
    private readonly ILogger<VectorStoreService> _logger = NullLogger<VectorStoreService>.Instance;

    [Fact]
    public void AddChunk_ShouldThrowArgumentException_WhenEmbeddingIsNull()
    {
        // Arrange
        var service = new VectorStoreService(_logger);
        var chunk = new DocumentChunk
        {
            Content = "Test content",
            Source = "test.md",
            ChunkIndex = 0,
            Embedding = null!
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.AddChunk(chunk));
    }

    [Fact]
    public void Search_ShouldReturnEmpty_WhenStoreIsEmpty()
    {
        // Arrange
        var service = new VectorStoreService(_logger);
        var queryEmbedding = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        var results = service.Search(queryEmbedding, 2);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Search_ShouldReturnTopKSimilarChunks()
    {
        // Arrange
        var service = new VectorStoreService(_logger);
        
        var chunkA = new DocumentChunk
        {
            Content = "Content A",
            Source = "a.md",
            ChunkIndex = 0,
            Embedding = new float[] { 1.0f, 0.0f, 0.0f } // Unit vector pointing along X axis
        };

        var chunkB = new DocumentChunk
        {
            Content = "Content B",
            Source = "b.md",
            ChunkIndex = 1,
            Embedding = new float[] { 0.0f, 1.0f, 0.0f } // Unit vector pointing along Y axis
        };

        service.AddChunk(chunkA);
        service.AddChunk(chunkB);

        // Query vector points along X axis (should be highly similar to A, orthogonal to B)
        var query = new float[] { 1.0f, 0.0f, 0.0f };

        // Act
        var results = service.Search(query, 2);

        // Assert
        Assert.Equal(2, results.Count);
        
        // Highly similar chunk A should be first (score = 1.0)
        Assert.Equal("Content A", results[0].Chunk.Content);
        Assert.Equal(1.0, results[0].Score, 5);

        // Orthogonal chunk B should be second (score = 0.0)
        Assert.Equal("Content B", results[1].Chunk.Content);
        Assert.Equal(0.0, results[1].Score, 5);
    }
}