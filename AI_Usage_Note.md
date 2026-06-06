# AI Collaboration & Usage Note

This document summarizes the interaction and collaboration with Antigravity (AI coding assistant) during the design, development, testing, and deployment of the **RAG Chat API**.

---

## 1. What AI Helped With
* **Scaffolding and Architecture Design**: Assisted in creating the core modular service structure of the Retrieval-Augmented Generation (RAG) pipeline:
  * `DocumentLoaderService` for Markdown text stripping.
  * `TextChunkingService` for semantic document segmentation.
  * `VectorStoreService` for memory-efficient cosine-similarity searching.
* **OpenAI Integration**: Refactored the local Ollama-only services to support **OpenAI** cloud API models (`gpt-4o-mini` and `text-embedding-3-small`) to enable cost-effective, high-performance cloud hosting.
* **xUnit Test Suites**: Assisted in setting up the xUnit test project (`RagApi.Tests`) and wrote happy-path unit tests covering chunking heuristics, HTML tag stripping, and cosine similarity calculations.
* **Render Cloud Deployment**: Diagnosed Docker compilation and deployment errors on Render, helped configure the Root Directory builds, and added detailed environment variable fallback bindings.

---

## 2. What AI Got Wrong & How It Was Resolved
* **PowerShell Command Escaping**: The AI attempted to run a `curl` query with embedded JSON headers. PowerShell failed to parse double-quotes within double-quotes (`\"\"`).
  * *Resolution*: Created a temporary `query.json` configuration file and loaded it into `curl.exe -d "@path"` to bypass Windows shell escaping.
* **PowerShell Cmdlet Alias**: The AI used the raw `curl` keyword, which defaults to PowerShell’s slow `Invoke-WebRequest` cmdlet, causing tasks to hang.
  * *Resolution*: Explicitly called the native system binary `curl.exe`.
* **Missing Runtime Tools in Docker**: The AI left a `HEALTHCHECK` directive calling `curl` in the base `Dockerfile`. Because base .NET runtime images do not have `curl` pre-installed, deployments failed.
  * *Resolution*: Removed the `HEALTHCHECK` block from the `Dockerfile` since Render handles health checks at the platform layer.
* **Required Properties in Tests**: Initialized `DocumentChunk` items in test methods without setting the C# 11 `required` property `ChunkIndex`, resulting in compiler errors.
  * *Resolution*: Fixed the object initializers in the unit tests.

---

## 3. Best Prompts Used
* *"run the entire project and give me the output page"* — Allowed the AI to execute the program and query endpoints in the background to verify the API's runtime status.
* *"i need to deploy and host it online"* — Triggered the discussion on VPS hosting vs PaaS (Render) and the necessary OpenAI API code migration.
* *"i want this procedures in the given image to be satisfied during the upload of the project"* — Provided the final checklist of academic requirements, triggering test suite creation and documentation additions.
