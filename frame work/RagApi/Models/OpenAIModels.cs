using System.Text.Json.Serialization;

namespace RagApi.Models;

/// <summary>
/// DTO representing an OpenAI embedding request.
/// </summary>
public sealed class OpenAIEmbeddingRequest
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}

/// <summary>
/// DTO representing an OpenAI embedding response.
/// </summary>
public sealed class OpenAIEmbeddingResponse
{
    [JsonPropertyName("data")]
    public List<OpenAIEmbeddingData> Data { get; set; } = new();
}

public sealed class OpenAIEmbeddingData
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

/// <summary>
/// DTO representing an OpenAI chat request.
/// </summary>
public sealed class OpenAIChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OpenAIChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;
}

public sealed class OpenAIChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// DTO representing an OpenAI chat response.
/// </summary>
public sealed class OpenAIChatResponse
{
    [JsonPropertyName("choices")]
    public List<OpenAIChatChoice> Choices { get; set; } = new();
}

public sealed class OpenAIChatChoice
{
    [JsonPropertyName("message")]
    public OpenAIChatMessage Message { get; set; } = new();
}
