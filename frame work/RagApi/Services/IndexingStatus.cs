namespace RagApi.Services;

/// <summary>
/// Keeps track of the startup document indexing status and errors.
/// </summary>
public sealed class IndexingStatus
{
    public bool IsComplete { get; set; }
    public int SuccessCount { get; set; }
    public string? LastError { get; set; }
}
