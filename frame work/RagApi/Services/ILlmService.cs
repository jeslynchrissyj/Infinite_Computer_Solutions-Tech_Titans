namespace RagApi.Services;

/// <summary>
/// Generates natural-language responses from an LLM.
/// Implement this interface for each LLM provider (Ollama, OpenAI, etc.).
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Sends a conversation (system prompt + user message) to the LLM and returns the assistant's reply.
    /// </summary>
    /// <param name="systemPrompt">The system-level instruction for the LLM.</param>
    /// <param name="userMessage">The user's message or question with context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The LLM-generated response text.</returns>
    Task<string> GenerateResponseAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);
}
