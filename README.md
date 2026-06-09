# 🤖 RAG Chat API — ASP.NET Core 8

A production-ready **Retrieval-Augmented Generation (RAG)** API built with ASP.NET Core 8 Minimal APIs. Ask natural-language questions and get answers grounded in your local Markdown documents, powered by Ollama (qwen2:1.5b + nomic-embed-text) or OpenAI.

### 🌐 Live Demo URLs
* **Live Swagger UI (Interactive Playground)**: [https://rag-chat-api-dotnet.onrender.com/swagger](https://rag-chat-api-dotnet.onrender.com/swagger)
* **Live API Health Check**: [https://rag-chat-api-dotnet.onrender.com/health](https://rag-chat-api-dotnet.onrender.com/health)

---

## 📦 Submission Package Deliverables

This repository contains all technical deliverables required for the project submission:
1. **Source Code**: Fully functional ASP.NET Core 8 Web API located in [frame work/RagApi](file:///frame%20work/RagApi).
2. **xUnit Tests**: Located in [frame work/RagApi.Tests](file:///frame%20work/RagApi.Tests) (9 tests covering semantic chunking, markdown parsing, and cosine similarity).
3. **AI Usage Note**: Documented in [AI_Usage_Note.md](file:///AI_Usage_Note.md) at the repository root.
4. **Sample Data**: Located in [sample_data/](file:///sample_data/) folder containing input documentation and expected JSON output payloads.

---

## Architecture

```
┌─────────────┐     ┌──────────────────────────────────────────────────┐
│  Client      │────▶│  POST /chat  { "question": "..." }              │
│  (curl/UI)   │◀────│  ──────────────────────────────────────────────  │
└─────────────┘     │  ASP.NET Core 8 Minimal API                     │
                    │                                                  │
                    │  ┌─────────────┐  ┌─────────────────────────┐   │
                    │  │ RagService   │──│ 1. Embed question       │   │
                    │  │              │  │ 2. Vector search        │   │
                    │  │              │  │ 3. Build context        │   │
                    │  │              │  │ 4. LLM generation       │   │
                    │  └──────┬───────┘  └─────────────────────────┘   │
                    │         │                                        │
                    │  ┌──────┴───────────────────────────────┐       │
                    │  │  IEmbeddingService  │  ILlmService    │       │
                    │  │  (OllamaEmbedding)  │  (OllamaLlm)    │       │
                    │  └──────┬──────────────┴────────┬────────┘       │
                    │         │                       │                │
                    └─────────┼───────────────────────┼────────────────┘
                              │                       │
                    ┌─────────▼───────────────────────▼────────┐
                    │              Ollama (localhost:11434)      │
                    │  ┌──────────────┐  ┌──────────────────┐  │
                    │  │ nomic-embed  │  │   qwen2:1.5b     │  │
                    │  │   -text      │  │                  │  │
                    │  └──────────────┘  └──────────────────┘  │
                    └──────────────────────────────────────────┘
```

## RAG Pipeline Flow

```
User Question
      │
      ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Generate    │────▶│   Vector    │────▶│   Build     │
│  Embedding   │     │   Search    │     │   Context   │
└─────────────┘     └─────────────┘     └──────┬──────┘
                                               │
                                               ▼
                                         ┌─────────────┐
                                         │  LLM Call   │
                                         │(qwen2:1.5b) │
                                         └──────┬──────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │  Response   │
                                        │  + Sources  │
                                        └─────────────┘
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Ollama](https://ollama.ai/) installed and running locally

### Install Required Ollama Models

```bash
ollama pull qwen2:1.5b
ollama pull nomic-embed-text
```

Verify Ollama is running:

```bash
curl http://localhost:11434/api/tags
```

---

## Project Structure

```
RagApi/
├── Documents/                      # Knowledge base (Markdown files)
│   ├── aspnet-core-overview.md
│   ├── dependency-injection.md
│   └── minimal-apis.md
├── Models/
│   ├── ChatRequest.cs              # API request DTO
│   ├── ChatResponse.cs             # API response DTO
│   ├── DocumentChunk.cs            # Internal chunk model
│   └── OllamaModels.cs             # Ollama API DTOs
├── Services/
│   ├── IEmbeddingService.cs        # Embedding abstraction
│   ├── ILlmService.cs              # LLM abstraction
│   ├── OllamaEmbeddingService.cs   # Ollama embedding implementation
│   ├── OllamaLlmService.cs         # Ollama chat implementation
│   ├── DocumentLoaderService.cs    # Markdown file loader
│   ├── TextChunkingService.cs      # Text splitter
│   ├── VectorStoreService.cs       # In-memory vector store
│   ├── RagService.cs               # RAG orchestrator
│   └── DocumentIndexingService.cs  # Startup indexing service
├── appsettings.json                # Configuration
├── appsettings.Development.json
├── Program.cs                      # Entry point
├── RagApi.csproj                   # Project file
├── Dockerfile
├── .dockerignore
└── README.md
```

---

## Quick Start

### 1. Clone and Navigate

```bash
cd "frame work/RagApi"
```

### 2. Restore and Build

```bash
dotnet restore
dotnet build
```

### 3. Run

```bash
dotnet run
```

The API will:
1. Load all Markdown files from the `Documents/` folder
2. Split them into text chunks
3. Generate embeddings via Ollama
4. Build the in-memory vector index
5. Start listening on `http://localhost:5000`

### 4. Open Swagger UI

Navigate to: **http://localhost:5000/swagger**

### 5. Send a Request

```bash
curl -X POST http://localhost:5000/chat \
  -H "Content-Type: application/json" \
  -d '{"question": "What is dependency injection?"}'
```

---

## Sample Request & Response

### Request

```json
POST /chat
Content-Type: application/json

{
  "question": "What is dependency injection and what are the service lifetimes in ASP.NET Core?"
}
```

### Response

```json
{
  "question": "What is dependency injection and what are the service lifetimes in ASP.NET Core?",
  "answer": "Dependency Injection (DI) is a software design pattern that implements Inversion of Control (IoC) for resolving dependencies. Instead of a class creating its own dependencies, they are provided from the outside by a DI container.\n\nASP.NET Core's built-in DI container supports three service lifetimes:\n\n1. **Transient**: Created each time they are requested. Best for lightweight, stateless services.\n2. **Scoped**: Created once per HTTP request. Shared within a single request but not across requests.\n3. **Singleton**: Created only once for the lifetime of the application. Shared across all requests and threads.\n\nThe DI container automatically resolves and injects all constructor parameters when creating service instances.",
  "sources": [
    "dependency-injection.md",
    "aspnet-core-overview.md"
  ],
  "sourceDetails": [
    {
      "source": "dependency-injection.md",
      "chunkIndex": 0,
      "score": 0.9854,
      "preview": "Dependency injection is a software design pattern where dependencies between classes..."
    }
  ]
}
```

---

## Configuration

All settings are in `appsettings.json`:

| Setting | Default | Description |
|---|---|---|
| `LlmProvider` | `"Ollama"` | LLM provider (`Ollama` or `OpenAI`) |
| `Ollama:BaseUrl` | `http://localhost:11434` | Ollama server URL |
| `Ollama:EmbeddingModel` | `nomic-embed-text` | Model for embedding generation |
| `Ollama:ChatModel` | `qwen2:1.5b` | Model for text generation |
| `Rag:DocumentsPath` | `Documents` | Path to Markdown knowledge base |
| `Rag:ChunkSize` | `500` | Max characters per chunk |
| `Rag:ChunkOverlap` | `50` | Overlap between consecutive chunks |
| `Rag:TopK` | `3` | Number of chunks to retrieve per query |
| `Rag:MaxContextLength` | `4000` | Max context length sent to LLM |

---

## Adding Your Own Documents

You can add documents to the RAG knowledge base using two methods:

### Method A: Dynamic Upload (via API Endpoint)
Upload a Markdown file dynamically without restarting the application:

* **Local:**
  ```bash
  curl -X POST "http://localhost:5000/documents?chunkSize=500&chunkOverlap=50" \
    -H "Content-Type: multipart/form-data" \
    -F "file=@security-policy.md"
  ```
* **Render (Live):**
  ```bash
  curl -X POST "https://rag-chat-api-dotnet.onrender.com/documents?chunkSize=500&chunkOverlap=50" \
    -H "Content-Type: multipart/form-data" \
    -F "file=@security-policy.md"
  ```

Or use the interactive upload option directly under the `/documents` route in **Swagger UI**.

### Method B: Startup Load (Static Folder)
1. Place `.md` files in the `Documents/` folder.
2. Start or restart the application.
3. Chunks are automatically loaded, chunked, embedded, and indexed on startup.

---

## Inspecting Indexed Chunks
To view all chunks currently loaded in the database, call:

* **Local:**
  ```bash
  curl http://localhost:5000/documents
  ```
* **Render (Live):**
  ```bash
  curl https://rag-chat-api-dotnet.onrender.com/documents
  ```

This returns a list of indexed chunks with their metadata: source file, chunk index, text length, embedding dimensions, and snippet preview.

---

## Docker

### Build

```bash
docker build -t rag-api .
```

### Run

```bash
docker run -p 5000:5000 \
  -e Ollama__BaseUrl=http://host.docker.internal:11434 \
  rag-api
```

> **Note**: Use `host.docker.internal` to reach Ollama running on the host machine.

---

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Swashbuckle.AspNetCore` | 6.5.0 | Swagger/OpenAPI documentation |
| `Markdig` | 0.37.0 | Markdown to plain text conversion |
| `Microsoft.Extensions.Http` | 8.0.1 | Typed HttpClient factory |

---

## Extending with OpenAI

The architecture is designed for easy provider swapping. You can choose to run with OpenAI models by setting `"LlmProvider": "OpenAI"` in your configurations and supplying your API key either in `appsettings.json` or as an environment variable named `OPENAI_API_KEY`.

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/chat` | Submit a question for RAG-powered answers |
| `GET` | `/health` | Health check with indexed chunk count |

---

## Assumptions & Limitations

### Assumptions
* **Knowledge Base Source**: Markdown (`.md`) documents are assumed to be well-formed, UTF-8 encoded text.
* **OpenAI Availability**: When `LlmProvider` is set to `OpenAI`, the host server must have outbound internet connectivity and a valid API key configured.
* **Ollama Endpoint**: When running locally under the default `Ollama` provider, Ollama is assumed to be active on `http://localhost:11434` with the `qwen2:1.5b` and `nomic-embed-text` models pre-pulled.

### Limitations
* **In-Memory Store**: The vector store runs entirely in RAM. Restarting the API clears the index, which must then be re-built on startup.
* **Parsing Complexity**: The document parser strips raw HTML and Markdown formatting. It does not extract images, handle complex tables, or retain mathematical notations.
* **Local Latency**: Generating embeddings and responses on local CPU hardware via Ollama can be slow depending on system specifications.

---

## Unit Testing

The project contains a test suite built with **xUnit** covering the core logic of text chunking, document loading, and cosine-similarity searches.

### Run Tests
To run the tests:
1. Navigate to the test project directory:
   ```bash
   cd "frame work/RagApi.Tests"
   ```
2. Run the test command:
   ```bash
   dotnet test
   ```

---

## License

This project is provided as an educational example of enterprise-grade RAG in .NET.
