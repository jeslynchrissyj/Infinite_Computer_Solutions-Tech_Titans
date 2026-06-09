namespace RagApi.Models;

/// <summary>
/// Represents the API response containing the generated answer and its sources.
/// </summary>
public sealed record ChatResponse
{
    /// <summary>
    /// The original question from the user.
    /// </summary>
    public required string Question { get; init; }

    /// <summary>
    /// The AI-generated answer based on retrieved document context.
    /// </summary>
    public required string Answer { get; init; }

    /// <summary>
    /// List of source document filenames that contributed to the answer.
    /// </summary>
    public required List<string> Sources { get; init; }

    /// <summary>
    /// Detailed tracking of retrieved chunks, including similarity scores.
    /// </summary>
    public List<SearchResultDetail>? SourceDetails { get; init; }
}

/// <summary>
/// Holds detailed parameters of a retrieved text chunk from vector search.
/// </summary>
public sealed record SearchResultDetail
{
    public required string Source { get; init; }
    public required int ChunkIndex { get; init; }
    public required double Score { get; init; }
    public required string Preview { get; init; }
}
