using RagApi.Models;

namespace RagApi.Services;

/// <summary>
/// Background service that runs at application startup to load, chunk, embed,
/// and index all Markdown documents before the API starts accepting requests.
/// Implements <see cref="IHostedService"/> so it integrates with the ASP.NET Core host lifecycle.
/// </summary>
public sealed class DocumentIndexingService : IHostedService
{
    private readonly DocumentLoaderService _documentLoader;
    private readonly TextChunkingService _textChunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly VectorStoreService _vectorStore;
    private readonly IndexingStatus _indexingStatus;
    private readonly ILogger<DocumentIndexingService> _logger;

    public DocumentIndexingService(
        DocumentLoaderService documentLoader,
        TextChunkingService textChunker,
        IEmbeddingService embeddingService,
        VectorStoreService vectorStore,
        IndexingStatus indexingStatus,
        ILogger<DocumentIndexingService> logger)
    {
        _documentLoader = documentLoader;
        _textChunker = textChunker;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _indexingStatus = indexingStatus;
        _logger = logger;
    }

    /// <summary>
    /// Runs the full document indexing pipeline at startup:
    /// Load → Chunk → Embed → Store.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _indexingStatus.IsComplete = false;
        _indexingStatus.SuccessCount = 0;
        _indexingStatus.LastError = null;

        _logger.LogInformation("╔══════════════════════════════════════════════════╗");
        _logger.LogInformation("║    Document Indexing Service — Starting...       ║");
        _logger.LogInformation("╚══════════════════════════════════════════════════╝");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Phase 1: Load documents from disk
            _logger.LogInformation("Phase 1/3: Loading documents from disk...");
            var documents = await _documentLoader.LoadDocumentsAsync(cancellationToken);

            if (documents.Count == 0)
            {
                _logger.LogWarning("No documents found to index. The API will start without a knowledge base.");
                _indexingStatus.IsComplete = true;
                return;
            }

            // Phase 2: Split documents into chunks
            _logger.LogInformation("Phase 2/3: Splitting {Count} document(s) into chunks...", documents.Count);
            var allChunks = new List<DocumentChunk>();

            foreach (var doc in documents)
            {
                var textChunks = _textChunker.SplitText(doc.Content);
                foreach (var tc in textChunks)
                {
                    allChunks.Add(new DocumentChunk
                    {
                        Content = tc.Content,
                        Source = doc.FileName,
                        ChunkIndex = tc.Index
                    });
                }
            }

            _logger.LogInformation("Created {Count} chunk(s) from {DocCount} document(s)",
                allChunks.Count, documents.Count);

            // Phase 3: Generate embeddings and store in vector store
            _logger.LogInformation("Phase 3/3: Generating embeddings for {Count} chunk(s)...", allChunks.Count);
            var embeddedCount = 0;

            foreach (var chunk in allChunks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    chunk.Embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content, cancellationToken);
                    _vectorStore.AddChunk(chunk);
                    embeddedCount++;
                    _indexingStatus.SuccessCount = embeddedCount;

                    if (embeddedCount % 10 == 0 || embeddedCount == allChunks.Count)
                    {
                        _logger.LogInformation("Embedded {Current}/{Total} chunk(s)...",
                            embeddedCount, allChunks.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to embed chunk {Index} from '{Source}'",
                        chunk.ChunkIndex, chunk.Source);
                    _indexingStatus.LastError = ex.Message;
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("╔══════════════════════════════════════════════════╗");
            _logger.LogInformation("║    Indexing Complete!                             ║");
            _logger.LogInformation("║    Documents: {Docs,-5} Chunks: {Chunks,-5} Time: {Time}  ║",
                documents.Count, embeddedCount, stopwatch.Elapsed.ToString(@"mm\:ss\.fff"));
            _logger.LogInformation("╚══════════════════════════════════════════════════╝");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Document indexing was cancelled during startup.");
            _indexingStatus.LastError = "Document indexing was cancelled during startup.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document indexing failed. The API will start without a knowledge base.");
            _indexingStatus.LastError = ex.Message;
        }
        finally
        {
            _indexingStatus.IsComplete = true;
        }
    }

    /// <summary>
    /// No cleanup required on shutdown.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
