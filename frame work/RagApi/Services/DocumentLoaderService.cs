using Markdig;

namespace RagApi.Services;

/// <summary>
/// Loads Markdown documents from disk and converts them to plain text.
/// Uses Markdig to strip Markdown formatting.
/// </summary>
public sealed class DocumentLoaderService
{
    private readonly string _documentsPath;
    private readonly ILogger<DocumentLoaderService> _logger;

    public DocumentLoaderService(IConfiguration configuration, ILogger<DocumentLoaderService> logger)
    {
        // Resolve the documents path relative to the content root
        var relativePath = configuration["Rag:DocumentsPath"] ?? "Documents";
        _documentsPath = Path.GetFullPath(relativePath);
        _logger = logger;
    }

    /// <summary>
    /// Represents a loaded document with its source filename and plain-text content.
    /// </summary>
    public sealed record LoadedDocument(string FileName, string Content);

    /// <summary>
    /// Reads all <c>.md</c> files from the configured documents directory
    /// and returns them as plain-text documents.
    /// </summary>
    /// <returns>A list of loaded documents with Markdown formatting removed.</returns>
    public async Task<List<LoadedDocument>> LoadDocumentsAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_documentsPath))
        {
            _logger.LogWarning("Documents directory not found at '{Path}'. No documents will be loaded.", _documentsPath);
            return new List<LoadedDocument>();
        }

        var markdownFiles = Directory.GetFiles(_documentsPath, "*.md", SearchOption.AllDirectories);
        _logger.LogInformation("Found {Count} Markdown file(s) in '{Path}'", markdownFiles.Length, _documentsPath);

        var documents = new List<LoadedDocument>();

        foreach (var filePath in markdownFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
                var plainText = ConvertMarkdownToPlainText(markdown);

                if (string.IsNullOrWhiteSpace(plainText))
                {
                    _logger.LogWarning("Skipping empty document: {File}", filePath);
                    continue;
                }

                var fileName = Path.GetFileName(filePath);
                documents.Add(new LoadedDocument(fileName, plainText));
                _logger.LogDebug("Loaded document '{File}' ({Length} characters)", fileName, plainText.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load document: {File}", filePath);
            }
        }

        _logger.LogInformation("Successfully loaded {Count} document(s)", documents.Count);
        return documents;
    }

    /// <summary>
    /// Converts Markdown to plain text by rendering to HTML and then stripping tags.
    /// This preserves the logical text content while removing all formatting.
    /// </summary>
    public static string ConvertMarkdownToPlainText(string markdown)
    {
        // Build a Markdig pipeline that handles common Markdown extensions
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        // Render Markdown to HTML, then strip HTML tags to get plain text
        var html = Markdown.ToHtml(markdown, pipeline);
        var plainText = StripHtmlTags(html);

        // Normalize whitespace: collapse multiple newlines, trim lines
        var lines = plainText
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line));

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Removes HTML tags from a string, leaving only the text content.
    /// </summary>
    private static string StripHtmlTags(string html)
    {
        // Simple but effective HTML tag removal using spans for performance
        var result = new System.Text.StringBuilder(html.Length);
        var insideTag = false;

        foreach (var c in html)
        {
            switch (c)
            {
                case '<':
                    insideTag = true;
                    break;
                case '>':
                    insideTag = false;
                    result.Append(' '); // Replace tag with space to avoid word concatenation
                    break;
                default:
                    if (!insideTag)
                        result.Append(c);
                    break;
            }
        }

        // Decode common HTML entities
        return result.ToString()
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&#39;", "'")
            .Replace("&nbsp;", " ");
    }
}
