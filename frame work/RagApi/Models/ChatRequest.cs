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
}
