namespace RagApi.Models;

/// <summary>
/// Represents an incoming chat request from the user.
/// </summary>
public sealed record ChatRequest
{
    /// <summary>
    /// The user's natural-language question.
    /// </summary>
    /// <example>What is dependency injection in .NET?</example>
    public required string Question { get; init; }

    /// <summary>
    /// Optional override for the number of top relevant chunks to retrieve (Top-K).
    /// </summary>
    /// <example>3</example>
    public int? TopK { get; init; }
}
