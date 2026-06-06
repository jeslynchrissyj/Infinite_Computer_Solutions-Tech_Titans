namespace RagApi.Services;

/// <summary>
/// Mock LLM service that returns predefined realistic responses for testing and offline cloud deployment.
/// </summary>
public sealed class MockLlmService : ILlmService
{
    public Task<string> GenerateResponseAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var lowerMessage = userMessage.ToLower();

        if (lowerMessage.Contains("dependency injection") || lowerMessage.Contains("lifetimes"))
        {
            return Task.FromResult(
                "Dependency Injection (DI) is a software design pattern that implements Inversion of Control (IoC) " +
                "for resolving dependencies in ASP.NET Core. Instead of a class creating its own dependencies, " +
                "they are provided from the outside by a built-in DI container.\n\n" +
                "The container supports three service lifetimes:\n" +
                "1. **Transient**: Created each time they are requested. Best for lightweight, stateless services.\n" +
                "2. **Scoped**: Created once per HTTP request. Shared within a single request but not across requests.\n" +
                "3. **Singleton**: Created only once for the lifetime of the application. Shared globally across all threads.");
        }

        if (lowerMessage.Contains("minimal api") || lowerMessage.Contains("minimal-api"))
        {
            return Task.FromResult(
                "Minimal APIs in ASP.NET Core 8 are designed to create HTTP APIs with minimal dependencies. " +
                "They are ideal for microservices and small apps that want to avoid the overhead of traditional " +
                "controllers. Routing and handlers are declared directly in Program.cs using method mappings like " +
                "MapGet() and MapPost().");
        }

        return Task.FromResult(
            "This is a grounded mock response from the RAG Chat API. " +
            "It is successfully pulling document contexts from your local Markdown database " +
            "and generating an answer based on your question: " + userMessage);
    }
}
