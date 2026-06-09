namespace RagApi.Services;

/// <summary>
/// Splits plain text into overlapping chunks suitable for embedding generation.
/// Attempts to respect paragraph boundaries to keep semantically coherent chunks.
/// </summary>
public sealed class TextChunkingService
{
    private readonly int _chunkSize;
    private readonly int _chunkOverlap;
    private readonly ILogger<TextChunkingService> _logger;

    public TextChunkingService(IConfiguration configuration, ILogger<TextChunkingService> logger)
    {
        _chunkSize = configuration.GetValue("Rag:ChunkSize", 500);
        _chunkOverlap = configuration.GetValue("Rag:ChunkOverlap", 50);
        _logger = logger;

        if (_chunkOverlap >= _chunkSize)
            throw new ArgumentException("ChunkOverlap must be less than ChunkSize.");
    }

    /// <summary>
    /// Represents a single chunk of text with its position index.
    /// </summary>
    public sealed record TextChunk(string Content, int Index);

    /// <summary>
    /// Splits the input text into overlapping chunks of approximately the specified size.
    /// Prefers to split at paragraph or sentence boundaries when possible.
    /// </summary>
    /// <param name="text">The plain text to split.</param>
    /// <param name="customChunkSize">Optional dynamic chunk character size limit override.</param>
    /// <param name="customChunkOverlap">Optional dynamic overlap character size override.</param>
    /// <returns>An ordered list of text chunks with their indices.</returns>
    public List<TextChunk> SplitText(string text, int? customChunkSize = null, int? customChunkOverlap = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<TextChunk>();

        var size = customChunkSize ?? _chunkSize;
        var overlap = customChunkOverlap ?? _chunkOverlap;

        if (overlap >= size)
            throw new ArgumentException("ChunkOverlap must be less than ChunkSize.");

        var chunks = new List<TextChunk>();
        var stepSize = size - overlap;
        var textLength = text.Length;
        var chunkIndex = 0;

        for (var start = 0; start < textLength; start += stepSize)
        {
            var end = Math.Min(start + size, textLength);
            var chunkText = text[start..end];

            // If this is not the last chunk, try to find a clean break point
            if (end < textLength)
            {
                var breakPoint = FindBestBreakPoint(chunkText);
                if (breakPoint > 0)
                {
                    chunkText = chunkText[..breakPoint];
                }
            }

            chunkText = chunkText.Trim();
            if (!string.IsNullOrWhiteSpace(chunkText))
            {
                chunks.Add(new TextChunk(chunkText, chunkIndex++));
            }
        }

        _logger.LogDebug("Split text of {Length} characters into {Count} chunk(s) " +
                          "(size={ChunkSize}, overlap={Overlap})",
            textLength, chunks.Count, size, overlap);

        return chunks;
    }

    /// <summary>
    /// Finds the best break point in a chunk by looking for paragraph breaks,
    /// then sentence endings, then word boundaries — searching from the end backwards.
    /// </summary>
    private static int FindBestBreakPoint(string text)
    {
        // Prefer paragraph breaks (double newline)
        var paragraphBreak = text.LastIndexOf("\n\n", StringComparison.Ordinal);
        if (paragraphBreak > text.Length / 2)
            return paragraphBreak;

        // Next, prefer sentence endings
        var sentenceEnd = FindLastSentenceEnd(text);
        if (sentenceEnd > text.Length / 2)
            return sentenceEnd + 1; // Include the period/punctuation

        // Fall back to any word boundary (space)
        var lastSpace = text.LastIndexOf(' ');
        if (lastSpace > text.Length / 2)
            return lastSpace;

        // No good break point found; return -1 to use the full chunk
        return -1;
    }

    /// <summary>
    /// Finds the last sentence-ending punctuation followed by a space or end of text.
    /// </summary>
    private static int FindLastSentenceEnd(string text)
    {
        for (var i = text.Length - 1; i > 0; i--)
        {
            if (text[i] is '.' or '!' or '?')
            {
                // Ensure it looks like a real sentence end (followed by space, newline, or end)
                if (i == text.Length - 1 || char.IsWhiteSpace(text[i + 1]))
                    return i;
            }
        }
        return -1;
    }
}
