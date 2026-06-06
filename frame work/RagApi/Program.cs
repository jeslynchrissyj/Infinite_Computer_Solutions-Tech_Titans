using System.Reflection;
using System.Net.Http.Headers;
using RagApi.Models;
using RagApi.Services;

// ──────────────────────────────────────────────────────────────
//  Builder Configuration
// ──────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 5000
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// ── Swagger / OpenAPI ────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RAG Chat API",
        Version = "v1",
        Description = "Retrieval-Augmented Generation API powered by ASP.NET Core 8 and OpenAI/Ollama. " +
                      "Ask questions and get answers grounded in local Markdown documents.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "RAG API",
            Url = new Uri("https://github.com/dotnet/aspnetcore")
        }
    });

    // Include XML comments in Swagger UI
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── LLM Provider Registration (Ollama / OpenAI) ─────────────
var llmProvider = builder.Configuration.GetValue<string>("LlmProvider") ?? "Ollama";

if (llmProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
{
    var ollamaBaseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";

    // Register HttpClient for OllamaEmbeddingService
    builder.Services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
    {
        client.BaseAddress = new Uri(ollamaBaseUrl);
        client.Timeout = TimeSpan.FromMinutes(5); // Embedding can be slow on first run
    });

    // Register HttpClient for OllamaLlmService
    builder.Services.AddHttpClient<ILlmService, OllamaLlmService>(client =>
    {
        client.BaseAddress = new Uri(ollamaBaseUrl);
        client.Timeout = TimeSpan.FromMinutes(5); // LLM generation can take time
    });
}
else if (llmProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
{
    var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];
    if (string.IsNullOrWhiteSpace(openAiApiKey))
    {
        openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    }

    if (string.IsNullOrWhiteSpace(openAiApiKey))
    {
        throw new InvalidOperationException(
            "OpenAI API Key is missing. Please set it in 'appsettings.json' under 'OpenAI:ApiKey' " +
            "or as an environment variable named 'OPENAI_API_KEY'.");
    }

    // Register HttpClient for OpenAIEmbeddingService
    builder.Services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>(client =>
    {
        client.BaseAddress = new Uri("https://api.openai.com/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
        client.Timeout = TimeSpan.FromMinutes(2);
    });

    // Register HttpClient for OpenAILlmService
    builder.Services.AddHttpClient<ILlmService, OpenAILlmService>(client =>
    {
        client.BaseAddress = new Uri("https://api.openai.com/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
        client.Timeout = TimeSpan.FromMinutes(2);
    });
}
else if (llmProvider.Equals("Mock", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IEmbeddingService, MockEmbeddingService>();
    builder.Services.AddSingleton<ILlmService, MockLlmService>();
}
else
{
    throw new InvalidOperationException(
        $"LLM provider '{llmProvider}' is not supported. Currently supported: Ollama, OpenAI, Mock.");
}

// ── Application Services ─────────────────────────────────────
builder.Services.AddSingleton<IndexingStatus>();
builder.Services.AddSingleton<DocumentLoaderService>();
builder.Services.AddSingleton<TextChunkingService>();
builder.Services.AddSingleton<VectorStoreService>();
builder.Services.AddScoped<RagService>();

// ── Startup Indexing ─────────────────────────────────────────
builder.Services.AddHostedService<DocumentIndexingService>();

// ──────────────────────────────────────────────────────────────
//  App Pipeline
// ──────────────────────────────────────────────────────────────

var app = builder.Build();

// Enable Swagger in all environments for this demo project
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RAG Chat API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "RAG Chat API — Swagger";
});

// ──────────────────────────────────────────────────────────────
//  Endpoints
// ──────────────────────────────────────────────────────────────

// Health check endpoint.
app.MapGet("/health", (VectorStoreService vectorStore, IConfiguration config, IndexingStatus indexingStatus) =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        IndexedChunks = vectorStore.Count,
        LlmProvider = config["LlmProvider"] ?? "Ollama",
        Indexing = new
        {
            indexingStatus.IsComplete,
            indexingStatus.SuccessCount,
            LastError = indexingStatus.LastError ?? "None"
        }
    });
})
.WithName("HealthCheck")
.WithTags("System")
.WithOpenApi(operation =>
{
    operation.Summary = "Health check";
    operation.Description = "Returns the API health status and the number of indexed document chunks.";
    return operation;
})
.Produces(200);

// Main RAG chat endpoint. Accepts a question and returns an AI-generated answer
// grounded in the indexed Markdown documents.
app.MapPost("/chat", async (ChatRequest request, RagService ragService, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    // Validate input
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest(new { Error = "The 'question' field is required and cannot be empty." });
    }

    if (request.Question.Length > 2000)
    {
        return Results.BadRequest(new { Error = "Question exceeds the maximum length of 2000 characters." });
    }

    try
    {
        logger.LogInformation("Received chat request: '{Question}'", request.Question);
        var response = await ragService.AskAsync(request.Question, cancellationToken);
        return Results.Ok(response);
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Failed to communicate with the LLM provider.");
        return Results.Problem(
            title: "LLM Provider Error",
            detail: "Could not connect to the LLM provider. Ensure Ollama is running at the configured URL.",
            statusCode: 502);
    }
    catch (OperationCanceledException)
    {
        logger.LogWarning("Chat request was cancelled.");
        return Results.StatusCode(499); // Client Closed Request
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error processing chat request.");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An unexpected error occurred while processing your request.",
            statusCode: 500);
    }
})
.WithName("Chat")
.WithTags("RAG")
.WithOpenApi(operation =>
{
    operation.Summary = "Ask a question (RAG)";
    operation.Description = "Submit a natural-language question. The API retrieves relevant document chunks " +
                            "from the knowledge base, builds context, and generates an answer using the configured LLM.";
    return operation;
})
.Accepts<ChatRequest>("application/json")
.Produces<ChatResponse>(200)
.Produces(400)
.Produces(502);

// Redirect root URL to Swagger UI
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// ──────────────────────────────────────────────────────────────
//  Run
// ──────────────────────────────────────────────────────────────

app.Logger.LogInformation("RAG Chat API starting on http://localhost:5000");
app.Logger.LogInformation("Swagger UI available at http://localhost:5000/swagger");
app.Logger.LogInformation("LLM Provider: {Provider}", llmProvider);

app.Run();
