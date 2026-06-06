using System.Net.Http.Json;
using RagApi.Models;

namespace RagApi.Services;

/// <summary>
/// Generates text responses using the Ollama local chat API.
/// Calls <c>POST /api/chat</c> with the configured chat model in non-streaming mode.
/// </summary>
public sealed class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OllamaLlmService> _logger;

    public OllamaLlmService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OllamaLlmService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["Ollama:ChatModel"] ?? "llama3";
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GenerateResponseAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var request = new OllamaChatRequest
        {
            Model = _model,
            Stream = false,
            Messages = new List<OllamaChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userMessage }
            }
        };

        _logger.LogDebug("Sending chat request to Ollama model '{Model}'", _model);

        var response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);

        var answer = result?.Message?.Content
            ?? throw new InvalidOperationException("Ollama returned an empty chat response.");

        _logger.LogDebug("Received response of length {Length} from Ollama (duration: {Duration}ms)",
            answer.Length, result!.TotalDuration / 1_000_000);

        return answer;
    }
}
