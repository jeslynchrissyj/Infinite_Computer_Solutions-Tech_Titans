using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RagApi.Services;

namespace RagApi.Tests;

public class TextChunkingServiceTests
{
    private readonly ILogger<TextChunkingService> _logger = NullLogger<TextChunkingService>.Instance;

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenOverlapIsGreaterThanOrEqualSize()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Rag:ChunkSize", "100"},
            {"Rag:ChunkOverlap", "100"} // Equal to size
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TextChunkingService(configuration, _logger));
    }

    [Fact]
    public void SplitText_ShouldReturnEmpty_WhenTextIsEmptyOrWhitespace()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var service = new TextChunkingService(configuration, _logger);

        // Act
        var resultNull = service.SplitText(null!);
        var resultEmpty = service.SplitText("");
        var resultWhitespace = service.SplitText("   ");

        // Assert
        Assert.Empty(resultNull);
        Assert.Empty(resultEmpty);
        Assert.Empty(resultWhitespace);
    }

    [Fact]
    public void SplitText_ShouldSplitCorrectly_WhenTextIsLongerThanChunkSize()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Rag:ChunkSize", "20"},
            {"Rag:ChunkOverlap", "5"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        var service = new TextChunkingService(configuration, _logger);
        
        // Let's create a text with no punctuation first to see simple boundary splits
        var text = "abcdefghijklmnopqrstuvwxyz1234567890"; // 36 chars

        // Act
        var result = service.SplitText(text);

        // Assert
        Assert.NotEmpty(result);
        foreach (var chunk in result)
        {
            Assert.True(chunk.Content.Length <= 20, $"Chunk '{chunk.Content}' exceeds size of 20");
        }
    }

    [Fact]
    public void SplitText_ShouldRespectParagraphBreaks()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Rag:ChunkSize", "80"},
            {"Rag:ChunkOverlap", "5"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        var service = new TextChunkingService(configuration, _logger);
        
        var text = "This is a longer paragraph content that easily fits in the first chunk.\n\nParagraph 2 is here.";

        // Act
        var result = service.SplitText(text);

        // Assert
        Assert.NotEmpty(result);
        // The first chunk should break at the double newline
        Assert.Contains("This is a longer paragraph content", result[0].Content);
        Assert.DoesNotContain("Paragraph 2", result[0].Content);
    }
}
