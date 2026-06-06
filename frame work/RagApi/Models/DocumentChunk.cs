namespace RagApi.Models;

/// <summary>
/// Represents a chunk of text extracted from a document, along with its embedding vector.
/// </summary>
public sealed class DocumentChunk
{
    /// <summary>
    /// The plain-text content of this chunk.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// The source filename (relative path) from which this chunk was extracted.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Zero-based index of this chunk within its source document.
    /// </summary>
    public required int ChunkIndex { get; init; }

    /// <summary>
    /// The embedding vector generated for <see cref="Content"/>.
    /// Null until an embedding has been computed.
    /// </summary>
    public float[]? Embedding { get; set; }
}
