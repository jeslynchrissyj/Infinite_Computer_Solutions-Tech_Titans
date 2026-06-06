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
}
