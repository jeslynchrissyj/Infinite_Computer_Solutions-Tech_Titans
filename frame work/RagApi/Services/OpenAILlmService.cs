using System.Net.Http.Json;
using RagApi.Models;

namespace RagApi.Services;

/// <summary>
/// Generates responses using the OpenAI Chat Completion API.
/// </summary>
public sealed class OpenAILlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OpenAILlmService> _logger;

    public OpenAILlmService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAILlmService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GenerateResponseAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var request = new OpenAIChatRequest
        {
            Model = _model,
            Messages = new List<OpenAIChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userMessage }
            }
        };

        _logger.LogDebug("Sending chat request to OpenAI model '{Model}'", _model);

        var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>(cancellationToken: cancellationToken);

        var answer = result?.Choices?[0]?.Message?.Content
            ?? throw new InvalidOperationException("OpenAI returned an empty chat response.");

        _logger.LogDebug("Received response from OpenAI of length {Length}", answer.Length);

        return answer;
    }
}
