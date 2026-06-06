using System.Text.Json.Serialization;

namespace RagApi.Models;

// ──────────────────────────────────────────────
//  Ollama Embedding API models
// ──────────────────────────────────────────────

/// <summary>
/// Request body for the Ollama <c>POST /api/embeddings</c> endpoint.
/// </summary>
public sealed class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("prompt")]
    public required string Prompt { get; init; }
}

/// <summary>
/// Response body from the Ollama <c>POST /api/embeddings</c> endpoint.
/// </summary>
public sealed class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; init; } = Array.Empty<float>();
}

// ──────────────────────────────────────────────
//  Ollama Chat API models
// ──────────────────────────────────────────────

/// <summary>
/// Request body for the Ollama <c>POST /api/chat</c> endpoint.
/// </summary>
public sealed class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("messages")]
    public required List<OllamaChatMessage> Messages { get; init; }

    /// <summary>
    /// When <c>false</c>, the full response is returned in a single JSON object
    /// instead of being streamed token-by-token.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = false;
}

/// <summary>
/// A single message in the Ollama chat conversation.
/// </summary>
public sealed class OllamaChatMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}

/// <summary>
/// Response body from the Ollama <c>POST /api/chat</c> endpoint (non-streaming).
/// </summary>
public sealed class OllamaChatResponse
{
    [JsonPropertyName("message")]
    public OllamaChatMessage? Message { get; init; }

    [JsonPropertyName("done")]
    public bool Done { get; init; }

    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; init; }
}
