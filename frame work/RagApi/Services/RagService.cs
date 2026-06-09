using RagApi.Models;

namespace RagApi.Services;

/// <summary>
/// Orchestrates the full Retrieval-Augmented Generation (RAG) pipeline:
/// query embedding → vector search → context assembly → LLM generation.
/// </summary>
public sealed class RagService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ILlmService _llmService;
    private readonly VectorStoreService _vectorStore;
    private readonly int _topK;
    private readonly int _maxContextLength;
    private readonly ILogger<RagService> _logger;

    /// <summary>
    /// System prompt that instructs the LLM to behave as a RAG assistant.
    /// </summary>
    private const string SystemPrompt = """
        You are a helpful, accurate assistant. Answer the user's question based ONLY on the 
        provided context below. If the context does not contain enough information to answer 
        the question, say "I don't have enough information to answer that question based on 
        the available documents."

        Rules:
        - Be concise and factual.
        - Do not make up information.
        - Reference the source documents when appropriate.
        - If the question is unrelated to the context, politely say so.
        """;

    public RagService(
        IEmbeddingService embeddingService,
        ILlmService llmService,
        VectorStoreService vectorStore,
        IConfiguration configuration,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _llmService = llmService;
        _vectorStore = vectorStore;
        _topK = configuration.GetValue("Rag:TopK", 3);
        _maxContextLength = configuration.GetValue("Rag:MaxContextLength", 4000);
        _logger = logger;
    }

    /// <summary>
    /// Processes a user question through the full RAG pipeline and returns a contextual answer.
    /// </summary>
    /// <param name="question">The user's natural-language question.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ChatResponse"/> with the answer and source documents.</returns>
    public async Task<ChatResponse> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing RAG query: '{Question}'", question);

        // Step 1: Generate embedding for the user's question
        _logger.LogDebug("Step 1/4: Generating query embedding...");
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question, cancellationToken);

        // Step 2: Search the vector store for relevant chunks
        _logger.LogDebug("Step 2/4: Searching vector store for top-{K} results...", _topK);
        var searchResults = _vectorStore.Search(queryEmbedding, _topK);

        if (searchResults.Count == 0)
        {
            _logger.LogWarning("No relevant documents found for the query.");
            return new ChatResponse
            {
                Question = question,
                Answer = "I couldn't find any relevant documents to answer your question. " +
                          "Please make sure the knowledge base has been loaded.",
                Sources = new List<string>(),
                SourceDetails = new List<SearchResultDetail>()
            };
        }

        _logger.LogDebug("Found {Count} relevant chunk(s). Scores: {Scores}",
            searchResults.Count,
            string.Join(", ", searchResults.Select(r => $"{r.Score:F4}")));

        // Step 3: Build context from retrieved chunks
        _logger.LogDebug("Step 3/4: Building context from retrieved chunks...");
        var (context, sources) = BuildContext(searchResults);

        // Step 4: Generate answer using the LLM
        _logger.LogDebug("Step 4/4: Generating answer with LLM...");
        var userMessage = $"""
            Context from knowledge base:
            ---
            {context}
            ---

            Question: {question}
            """;

        var answer = await _llmService.GenerateResponseAsync(SystemPrompt, userMessage, cancellationToken);

        _logger.LogInformation("RAG query completed. Sources: [{Sources}]",
            string.Join(", ", sources));

        var sourceDetails = searchResults.Select(r => new SearchResultDetail
        {
            Source = r.Chunk.Source,
            ChunkIndex = r.Chunk.ChunkIndex,
            Score = r.Score,
            Preview = r.Chunk.Content.Length > 200 
                ? r.Chunk.Content.Substring(0, 200) + "..." 
                : r.Chunk.Content
        }).ToList();

        return new ChatResponse
        {
            Question = question,
            Answer = answer,
            Sources = sources,
            SourceDetails = sourceDetails
        };
    }

    /// <summary>
    /// Builds a context string from the retrieved chunks, respecting the max context length.
    /// Also extracts the unique source filenames.
    /// </summary>
    private (string Context, List<string> Sources) BuildContext(
        List<(DocumentChunk Chunk, double Score)> searchResults)
    {
        var contextBuilder = new System.Text.StringBuilder();
        var sources = new HashSet<string>();
        var currentLength = 0;

        foreach (var (chunk, score) in searchResults)
        {
            // Build a context entry with source attribution
            var entry = $"[Source: {chunk.Source}]\n{chunk.Content}\n\n";

            // Respect max context length to avoid exceeding LLM token limits
            if (currentLength + entry.Length > _maxContextLength)
            {
                _logger.LogDebug("Context length limit reached at {Length}/{Max} characters",
                    currentLength, _maxContextLength);
                break;
            }

            contextBuilder.Append(entry);
            currentLength += entry.Length;
            sources.Add(chunk.Source);
        }

        return (contextBuilder.ToString(), sources.ToList());
    }
}
