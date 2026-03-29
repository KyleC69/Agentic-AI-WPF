Context Management Architecture
===============================

This document formalizes the strategies and architectural patterns used to manage context, memory, and retrieval within the system. It defines the lifecycle of conversation data, tool results, reasoning artifacts, and knowledge sources, ensuring deterministic behavior, efficient token usage, and reliable multi‑step agent execution.

* * *

1. Overview

* * *

Context management is built around a **sliding‑window model with explicit state**, not summarization or message‑count reduction. The system maintains a clean separation between:

* **Permanent conversation history** gated by compiler constant 'SQL'

* **Durable knowledge sources (RAG)**

* **Hybrid remote knowledge sources**

This layered approach ensures predictable behavior, efficient retrieval, and human‑like continuity.

* * *

2. Conversation History (Permanent Storage)

* * *

Conversation history is stored permanently in SQL using `user` and `assistant` roles. This provides:

* Deterministic replay of past interactions

* Auditability and debugging

* The ability to rebuild context on demand

* A stable foundation for context injection

Conversation history is **never summarized** and is the last to fall out of context as conversations grow. This allows model to mantain a rich memory of the users interactions, and provides a reliable source of truth for the conversation state. The history is injected into the model as needed, with the most recent interactions taking priority.

* * *

3. Semi‑Volatile File Based Conversation Cache

* * *

The conversation cache stores all previously injected context and tool results in a structured, searchable format. It acts as the system’s **working memory**.

### 3.1 Purpose

* Prevents repeated tool calls

* Prevents repeated web calls

* Eliminates replay of large, noisy tool results

* Keeps prompts small and focused

* Enables accurate responses to queries like “remember when…?”

### 3.2 Lifecycle

* Cache is **bounded by the conversation**

* Reset occurs when the user resets the conversation

* Cache persists across application restarts for the same conversation

### 3.3 Structure

Cache entries are stored as structured JSON objects, not raw tool output. Each entry includes:

* Extracted facts

* Summaries

* Metadata

* Embeddings (optional)

* Normalized fields for efficient search

This reduces token noise and improves retrieval accuracy.

* * *

4. Agent Progress Log (Planning State)

* * *

Long‑running plans require a durable, resumable state object. The progress log captures:

* Plan ID

* Step list

* Current step index

* Intermediate reasoning artifacts

* Partial tool results

* Completion status per step

### 4.1 Purpose

* Enables plan resumption after cancellation or failure

* Prevents divergence during multi‑step refactors

* Provides transparency into agent behavior

* Supports deterministic execution

### 4.2 Format

A typical progress log entry includes:
    {
      "planId": "...",
      "currentStep": 3,
      "steps": [
        { "id": 1, "status": "complete" },
        { "id": 2, "status": "complete" },
        { "id": 3, "status": "in_progress" }
      ],
      "artifacts": {
        "step2": { /* structured output */ },
        "step3": { /* partial output */ }
      }
    }

* * *

5. Context Injectors (Knowledge Sources)

* * *

Three injectors provide context to the model, ordered by cost and relevance.

### 5.1 Injector 1 — Local Conversation + Cache

* Fastest and most relevant

* Uses conversation history and semi‑volatile cache

* Provides human‑like continuity

### 5.2 Injector 2 — Local RAG (SQL)

* Stores ingested documents from the Agent Framework repository

* Provides stable, curated domain knowledge

* Uses semantic search + reranking

### 5.3 Injector 3 — Hybrid Remote Knowledge

* Pulls from remote sources such as MS Learn

* Locally indexes retrieved documents

* Ensures up‑to‑date technical knowledge

* * *

6. Retrieval Strategy

* * *

Retrieval follows a strict priority order:

1. **Conversation Cache** (semi‑volatile)

2. **Conversation History** (SQL)

3. **Local RAG Store** (SQL)

4. **Hybrid Remote Knowledge** (web + local index)

This ensures:

* Minimal external calls

* Minimal token usage

* High relevance

* Deterministic behavior

* * *

7. Cache and Log Maintenance

* * *

### 7.1 Cache Reset

* Occurs when the user resets the conversation

* Ensures clean state boundaries

### 7.2 Cache Decay (Optional Enhancements)

* Time‑based decay

* Relevance‑based pruning

* Size‑based limits

### 7.3 Progress Log Lifecycle

* Created at plan start

* Updated after each step

* Destroyed when plan completes or is abandoned

* * *

8. Design Principles

* * *

The system adheres to the following principles:

* **No summarization** unless explicitly requested

* **No replay of raw tool results**

* **Explicit state over implicit model memory**

* **Deterministic behavior** through structured storage

* **Efficient token usage** via curated context injection

* **Human‑like continuity** through persistent conversation history

* * *

9. Summary

* * *

This architecture provides a robust, scalable, and deterministic approach to context management. By combining permanent history, semi‑volatile working memory, durable plan state, and layered knowledge sources, the system achieves:

* Efficient retrieval

* Predictable behavior

* Accurate long‑running plans

* Human‑like conversational continuity

This document serves as the foundation for implementing context management within the repository.
