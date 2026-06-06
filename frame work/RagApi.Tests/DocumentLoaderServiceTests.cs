using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RagApi.Services;

namespace RagApi.Tests;

public class DocumentLoaderServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly ILogger<DocumentLoaderService> _logger = NullLogger<DocumentLoaderService>.Instance;

    public DocumentLoaderServiceTests()
    {
        // Setup a unique temporary directory for each test run
        _tempDirectory = Path.Combine(Path.GetTempPath(), "RagApiTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        // Cleanup the temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public async Task LoadDocumentsAsync_ShouldReturnEmpty_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "non_existent_folder");
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Rag:DocumentsPath", nonExistentPath}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        var service = new DocumentLoaderService(configuration, _logger);

        // Act
        var result = await service.LoadDocumentsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadDocumentsAsync_ShouldLoadAndStripMarkdown()
    {
        // Arrange
        // Create a couple of mock markdown files
        var file1Path = Path.Combine(_tempDirectory, "doc1.md");
        var file2Path = Path.Combine(_tempDirectory, "doc2.md");

        await File.WriteAllTextAsync(file1Path, "# Heading 1\nThis is a [link](http://example.com) and some **bold** text.");
        await File.WriteAllTextAsync(file2Path, "## Heading 2\n* Bullet item 1\n* Bullet item 2");

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Rag:DocumentsPath", _tempDirectory}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        var service = new DocumentLoaderService(configuration, _logger);

        // Act
        var result = await service.LoadDocumentsAsync();

        // Assert
        Assert.Equal(2, result.Count);

        var doc1 = result.FirstOrDefault(d => d.FileName == "doc1.md");
        Assert.NotNull(doc1);
        // Heading mark '#' should be stripped, bold marks '**' should be stripped, and link formatting stripped
        Assert.Contains("Heading 1", doc1.Content);
        Assert.Contains("This is a", doc1.Content);
        Assert.Contains("link", doc1.Content);
        Assert.Contains("bold", doc1.Content);
        Assert.Contains("text", doc1.Content);
        Assert.DoesNotContain("#", doc1.Content);
        Assert.DoesNotContain("[link]", doc1.Content);
        Assert.DoesNotContain("**", doc1.Content);

        var doc2 = result.FirstOrDefault(d => d.FileName == "doc2.md");
        Assert.NotNull(doc2);
        Assert.Contains("Heading 2", doc2.Content);
        Assert.Contains("Bullet item 1", doc2.Content);
        Assert.Contains("Bullet item 2", doc2.Content);
        Assert.DoesNotContain("*", doc2.Content);
    }
}
