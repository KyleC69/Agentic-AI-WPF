---
title: SQL Scripts
description: Reference list of SQL scripts used by the project.
date: 03/27/2026
---

# SQL Scripts

This folder contains SQL scripts for two main database areas:

- `AIDataRag`: RAG document storage, embedding generation, and search.
- `ChatHistory`: conversation storage, vectorization pipeline, and history search.

## Prerequisites

- SQL Server preview features used by AI functions must be enabled in your target database.
- External models must be created before embedding-generating procedures can run.

Example external model setup:

```sql
CREATE EXTERNAL MODEL MyOllamaModel
WITH (
      LOCATION = 'https://localhost:11434/api/embed',
      API_FORMAT = 'Ollama',
      MODEL_TYPE = EMBEDDINGS,
      MODEL = 'all-minilm'
);
```

## Recommended Execution Order

Use this order when provisioning a fresh environment.

1. Create databases and base schema.
2. Create core tables.
3. Create full-text catalogs.
4. Create external model objects.
5. Create user-defined functions.
6. Create stored procedures.
7. Seed or ingest sample content.
8. Validate vector and full-text search procedures.

Suggested script order by area:

### AIDataRag

1. [AIDataRag/AIDataRAG.Database.sql](AIDataRag/AIDataRAG.Database.sql)
2. [AIDataRag/dbo.md_documents.Table.sql](AIDataRag/dbo.md_documents.Table.sql)
3. [AIDataRag/dbo.md_document_chunks.Table.sql](AIDataRag/dbo.md_document_chunks.Table.sql)
4. [AIDataRag/dbo.RemoteRAG.Table.sql](AIDataRag/dbo.RemoteRAG.Table.sql)
5. [AIDataRag/MdDocsCatalog.FullTextCatalog.sql](AIDataRag/MdDocsCatalog.FullTextCatalog.sql)
6. [AIDataRag/MdChunksCatalog.FullTextCatalog.sql](AIDataRag/MdChunksCatalog.FullTextCatalog.sql)
7. [AIDataRag/RAGCatalog.FullTextCatalog.sql](AIDataRag/RAGCatalog.FullTextCatalog.sql)
8. [AIDataRag/RemoteRAG.FullTextCatalog.sql](AIDataRag/RemoteRAG.FullTextCatalog.sql)
9. [AIDataRag/MXEMBED.ExternalModel.sql](AIDataRag/MXEMBED.ExternalModel.sql)
10. [AIDataRag/dbo.sp_Generate_Embeddings.StoredProcedure.sql](AIDataRag/dbo.sp_Generate_Embeddings.StoredProcedure.sql)
11. [AIDataRag/dbo.sp_Update_md_Embeddings.StoredProcedure.sql](AIDataRag/dbo.sp_Update_md_Embeddings.StoredProcedure.sql)
12. [AIDataRag/dbo.sp_GenerateKeywords.StoredProcedure.sql](AIDataRag/dbo.sp_GenerateKeywords.StoredProcedure.sql)
13. [AIDataRag/dbo.sp_RecalculateRAGScores.StoredProcedure.sql](AIDataRag/dbo.sp_RecalculateRAGScores.StoredProcedure.sql)
14. [AIDataRag/dbo.sp_SafeCalculateRAGScores.StoredProcedure.sql](AIDataRag/dbo.sp_SafeCalculateRAGScores.StoredProcedure.sql)
15. [AIDataRag/dbo.Search_Vector.StoredProcedure.sql](AIDataRag/dbo.Search_Vector.StoredProcedure.sql)
16. [AIDataRag/dbo.Search_FullText.StoredProcedure.sql](AIDataRag/dbo.Search_FullText.StoredProcedure.sql)
17. [AIDataRag/dbo.Search_SemanticKeyPhrases.StoredProcedure.sql](AIDataRag/dbo.Search_SemanticKeyPhrases.StoredProcedure.sql)
18. [AIDataRag/dbo.Search_Hybrid.StoredProcedure.sql](AIDataRag/dbo.Search_Hybrid.StoredProcedure.sql)
19. [AIDataRag/dbo.Search_RAG_Advanced.StoredProcedure.sql](AIDataRag/dbo.Search_RAG_Advanced.StoredProcedure.sql)

### ChatHistory

1. [ChatHistory/dbo.ChatHistoryMessages.Table.sql](ChatHistory/dbo.ChatHistoryMessages.Table.sql)
2. [ChatHistory/dbo.ChatHistoryTextChunks.Table.sql](ChatHistory/dbo.ChatHistoryTextChunks.Table.sql)
3. [ChatHistory/ChatHistoryCat.FullTextCatalog.sql](ChatHistory/ChatHistoryCat.FullTextCatalog.sql)
4. [ChatHistory/ChatChunkIdx.FullTextCatalog.sql](ChatHistory/ChatChunkIdx.FullTextCatalog.sql)
5. [ChatHistory/dbo.fn_BM25.UserDefinedFunction.sql](ChatHistory/dbo.fn_BM25.UserDefinedFunction.sql)
6. [ChatHistory/dbo.sp_GenerateTextChunks.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateTextChunks.StoredProcedure.sql)
7. [ChatHistory/dbo.sp_GenerateEmbedding.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateEmbedding.StoredProcedure.sql)
8. [ChatHistory/dbo.sp_Generate_Vector.StoredProcedure.sql](ChatHistory/dbo.sp_Generate_Vector.StoredProcedure.sql)
9. [ChatHistory/dbo.sp_GenerateKeywords.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateKeywords.StoredProcedure.sql)
10. [ChatHistory/dbo.sp_GenerateSummary.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateSummary.StoredProcedure.sql)
11. [ChatHistory/dbo.sp_GetEmbedding.StoredProcedure.sql](ChatHistory/dbo.sp_GetEmbedding.StoredProcedure.sql)
12. [ChatHistory/dbo.sp_GetLastConversationId.StoredProcedure.sql](ChatHistory/dbo.sp_GetLastConversationId.StoredProcedure.sql)
13. [ChatHistory/dbo.sp_Search_FullText.StoredProcedure.sql](ChatHistory/dbo.sp_Search_FullText.StoredProcedure.sql)
14. [ChatHistory/dbo.sp_Search_Contains_FullText.StoredProcedure.sql](ChatHistory/dbo.sp_Search_Contains_FullText.StoredProcedure.sql)
15. [ChatHistory/dbo.sp_Search_FreeText.StoredProcedure.sql](ChatHistory/dbo.sp_Search_FreeText.StoredProcedure.sql)
16. [ChatHistory/dbo.sp_Search_FullText_Keywords.StoredProcedure.sql](ChatHistory/dbo.sp_Search_FullText_Keywords.StoredProcedure.sql)
17. [ChatHistory/dbo.sp_Search_Hybrid.StoredProcedure.sql](ChatHistory/dbo.sp_Search_Hybrid.StoredProcedure.sql)

## Safeguards

Use these safeguards when running scripts manually:

- Run scripts first in a development or staging environment, not production.
- Confirm you are connected to the intended database before executing each script.
- Keep schema scripts in source control and review diffs before applying changes.
- Prefer idempotent patterns such as `IF NOT EXISTS` for create operations when adapting scripts.
- Wrap destructive data changes in explicit transactions and verify row counts before commit.
- Back up the target database before running schema changes or recalculation procedures.
- Apply least-privilege SQL permissions for service accounts executing procedures.
- Validate external model endpoint reachability and TLS requirements before embedding jobs.
- After deployment, run smoke tests for table access, embedding generation, and search procedures.

## Stored Procedure Optimization Notes (03/27/2026)

The following safe optimizations were applied to procedure scripts in this repo:

- `AIDataRag/dbo.sp_Generate_Embeddings.StoredProcedure.sql`:
      - fixed `@Query` from `nvarchar` to `nvarchar(MAX)` to prevent silent single-character truncation
      - added non-empty input guard before embedding generation
- `AIDataRag/dbo.sp_GenerateKeywords.StoredProcedure.sql`:
      - switched local inference endpoint to `http://127.0.0.1:11434/api/generate`
      - removed debug payload/response resultsets to keep procedure output stable
      - added dual JSON-path handling (`$.response` and legacy `$.result.response`) and empty-response guard
- `AIDataRag/dbo.Search_Hybrid.StoredProcedure.sql`:
      - added guard for missing `@QueryEmbedding`
      - added fallback when tokenized full-text query is empty
      - normalized vector scoring when row embeddings are `NULL`
- `ChatHistory/dbo.sp_GetEmbedding.StoredProcedure.sql`:
      - changed input to `NVARCHAR(MAX)` and added non-empty input guard
- `ChatHistory/dbo.sp_GetLastConversationId.StoredProcedure.sql`:
      - made conversation selection deterministic with stable tie-break ordering
- `ChatHistory/dbo.sp_GenerateTextChunks.StoredProcedure.sql`:
      - added duplicate prevention logic for reruns
      - added inserted-row count output (`ChunksInserted`)

Execution impact:

- Procedure behavior is now stricter for empty inputs (procedures fail fast instead of generating low-quality data).
- Re-running chunk generation is now idempotent for previously generated chunk sets.
- Hybrid search behavior is more resilient when full-text tokenization yields no terms or stored embeddings are missing.

## AIDataRag Scripts

- [AIDataRag/AIDataRAG.Database.sql](AIDataRag/AIDataRAG.Database.sql) - Creates or configures the AIDataRAG database.
- [AIDataRag/dbo.md_document_chunks.Table.sql](AIDataRag/dbo.md_document_chunks.Table.sql) - Creates the markdown document chunk storage table.
- [AIDataRag/dbo.md_documents.Table.sql](AIDataRag/dbo.md_documents.Table.sql) - Creates the markdown document metadata/content table.
- [AIDataRag/dbo.RemoteRAG.Table.sql](AIDataRag/dbo.RemoteRAG.Table.sql) - Creates table for remote RAG content and related metadata.
- [AIDataRag/dbo.Search_FullText.StoredProcedure.sql](AIDataRag/dbo.Search_FullText.StoredProcedure.sql) - Full-text search stored procedure for RAG data.
- [AIDataRag/dbo.Search_Hybrid.StoredProcedure.sql](AIDataRag/dbo.Search_Hybrid.StoredProcedure.sql) - Hybrid search combining text and vector relevance.
- [AIDataRag/dbo.Search_RAG_Advanced.StoredProcedure.sql](AIDataRag/dbo.Search_RAG_Advanced.StoredProcedure.sql) - Advanced RAG search workflow procedure.
- [AIDataRag/dbo.Search_SemanticKeyPhrases.StoredProcedure.sql](AIDataRag/dbo.Search_SemanticKeyPhrases.StoredProcedure.sql) - Semantic key-phrase-based search procedure.
- [AIDataRag/dbo.Search_Vector.StoredProcedure.sql](AIDataRag/dbo.Search_Vector.StoredProcedure.sql) - Vector similarity search procedure.
- [AIDataRag/dbo.sp_Generate_Embeddings.StoredProcedure.sql](AIDataRag/dbo.sp_Generate_Embeddings.StoredProcedure.sql) - Generates embeddings for stored RAG content.
- [AIDataRag/dbo.sp_GenerateKeywords.StoredProcedure.sql](AIDataRag/dbo.sp_GenerateKeywords.StoredProcedure.sql) - Generates keywords for RAG content.
- [AIDataRag/dbo.sp_RecalculateRAGScores.StoredProcedure.sql](AIDataRag/dbo.sp_RecalculateRAGScores.StoredProcedure.sql) - Recalculates RAG scoring values.
- [AIDataRag/dbo.sp_SafeCalculateRAGScores.StoredProcedure.sql](AIDataRag/dbo.sp_SafeCalculateRAGScores.StoredProcedure.sql) - Safely recalculates RAG scores with defensive checks.
- [AIDataRag/dbo.sp_Update_md_Embeddings.StoredProcedure.sql](AIDataRag/dbo.sp_Update_md_Embeddings.StoredProcedure.sql) - Updates embedding vectors for markdown content.
- [AIDataRag/MdChunksCatalog.FullTextCatalog.sql](AIDataRag/MdChunksCatalog.FullTextCatalog.sql) - Creates full-text catalog for markdown chunks.
- [AIDataRag/MdDocsCatalog.FullTextCatalog.sql](AIDataRag/MdDocsCatalog.FullTextCatalog.sql) - Creates full-text catalog for markdown documents.
- [AIDataRag/MXEMBED.ExternalModel.sql](AIDataRag/MXEMBED.ExternalModel.sql) - Creates external embedding model definition.
- [AIDataRag/RAGCatalog.FullTextCatalog.sql](AIDataRag/RAGCatalog.FullTextCatalog.sql) - Creates full-text catalog used by RAG search.
- [AIDataRag/RemoteRAG.FullTextCatalog.sql](AIDataRag/RemoteRAG.FullTextCatalog.sql) - Creates full-text catalog for remote RAG table.

## ChatHistory Scripts

- [ChatHistory/ChatChunkIdx.FullTextCatalog.sql](ChatHistory/ChatChunkIdx.FullTextCatalog.sql) - Creates full-text catalog/index for chat text chunks.
- [ChatHistory/ChatHistoryCat.FullTextCatalog.sql](ChatHistory/ChatHistoryCat.FullTextCatalog.sql) - Creates full-text catalog for chat history content.
- [ChatHistory/dbo.ChatHistoryMessages.Table.sql](ChatHistory/dbo.ChatHistoryMessages.Table.sql) - Creates table for chat message records.
- [ChatHistory/dbo.ChatHistoryTextChunks.Table.sql](ChatHistory/dbo.ChatHistoryTextChunks.Table.sql) - Creates table for chunked chat text storage.
- [ChatHistory/dbo.fn_BM25.UserDefinedFunction.sql](ChatHistory/dbo.fn_BM25.UserDefinedFunction.sql) - Defines BM25 scoring function for ranking search results.
- [ChatHistory/dbo.sp_Generate_Vector.StoredProcedure.sql](ChatHistory/dbo.sp_Generate_Vector.StoredProcedure.sql) - Generates vectors for chat content.
- [ChatHistory/dbo.sp_GenerateEmbedding.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateEmbedding.StoredProcedure.sql) - Generates embeddings for chat messages/chunks.
- [ChatHistory/dbo.sp_GenerateKeywords.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateKeywords.StoredProcedure.sql) - Generates keywords from chat history content.
- [ChatHistory/dbo.sp_GenerateSummary.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateSummary.StoredProcedure.sql) - Produces summaries of chat history content.
- [ChatHistory/dbo.sp_GenerateTextChunks.StoredProcedure.sql](ChatHistory/dbo.sp_GenerateTextChunks.StoredProcedure.sql) - Splits chat content into chunks for indexing.
- [ChatHistory/dbo.sp_GetEmbedding.StoredProcedure.sql](ChatHistory/dbo.sp_GetEmbedding.StoredProcedure.sql) - Returns an embedding for a supplied input value.
- [ChatHistory/dbo.sp_GetLastConversationId.StoredProcedure.sql](ChatHistory/dbo.sp_GetLastConversationId.StoredProcedure.sql) - Gets the latest conversation ID.
- [ChatHistory/dbo.sp_Search_Contains_FullText.StoredProcedure.sql](ChatHistory/dbo.sp_Search_Contains_FullText.StoredProcedure.sql) - Full-text CONTAINS search over chat history.
- [ChatHistory/dbo.sp_Search_FreeText.StoredProcedure.sql](ChatHistory/dbo.sp_Search_FreeText.StoredProcedure.sql) - FREETEXT search over chat history.
- [ChatHistory/dbo.sp_Search_FullText_Keywords.StoredProcedure.sql](ChatHistory/dbo.sp_Search_FullText_Keywords.StoredProcedure.sql) - Full-text search tuned for keyword queries.
- [ChatHistory/dbo.sp_Search_FullText.StoredProcedure.sql](ChatHistory/dbo.sp_Search_FullText.StoredProcedure.sql) - General full-text search procedure.
- [ChatHistory/dbo.sp_Search_Hybrid.StoredProcedure.sql](ChatHistory/dbo.sp_Search_Hybrid.StoredProcedure.sql) - Hybrid search using text and semantic/vector signals.
*