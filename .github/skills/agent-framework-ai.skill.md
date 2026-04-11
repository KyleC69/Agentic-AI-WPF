---
name: agent-workflow-builder_ai_toolkit
description: Generates, enhances, develops, and deploys AI agent applications and workflows using Microsoft Agent Framework. Use when user asks to create, scaffold, build, modify, fix, trace, monitor, debug, evaluate, measure, or deploy AI apps, agents, or workflows.

---

# Building AI Agent / Workflow

## Critical Instructions

- **Interpret Intent**: Accurately capture user intent. Execute specific capabilities or multiple as needed. Ask if unclear.
- **SDK Usage**: Use **Microsoft Agent Framework** exclusively. DO NOT apply if user *explicitly* asks for other SDKs/packages.

## Core Responsibilities

1. **Agent Creation**: Generate AI agent code with best practices
2. **Existing Agent Enhancement**: Update, fix, add features, add debug support
3. **Model Selection**: Recommend and compare AI models
4. **Tracing**: Integrate tracing for debugging and monitoring
5. **Evaluation**: Assess agent performance and quality
6. **Deployment**: Go production via deploying to Foundry

## Core Principles

- **Language**: Use **Python** as the default programming language if unspecified.
- **Microsoft Agent Framework**: Unified Python/.NET SDK for enterprise AI agents and workflows with type safety, checkpointing, and multi-agent orchestration.

## Toolbelt

Use these tools to gather context and capabilities.

| Category | Tool | Description |
|----------|------|-------------|
| **Code Generation** | `aitk-get_agent_model_code_sample` | Get basic snippets (agent, workflow, chat) |
| **Code Generation** | `githubRepo` | Search `microsoft/agent-framework` for patterns (MCP, Multimodal, Assistants, etc.) |
| **Code Generation** | `aitk-agent_as_server` | Best practices for HTTP server wrapping (production-critical) |
| **Debugging** | `aitk-add_agent_debug` | Dev tools and VS Code configs for Agent Inspector |
| **Python Environment Setup** | `getPythonEnvironmentInfo`, `configurePythonEnvironment`, `installPythonPackage`, `getPythonExecutableCommand` | Manage Python environment and dependencies |
| **Models** | `aitk-get_ai_model_guidance` | Expert advice on model selection |
| **Models** | `aitk-list_foundry_models` | List deployed Foundry models |
| **Operations** | `aitk-get_tracing_code_gen_best_practices` | Tracing setup guidance |
| **Operations** | `aitk-evaluation_planner` | Evaluation strategy planning |
| **Operations** | `aitk-evaluation_agent_runner_best_practices` | Evaluation runner best practices |
| **Operations** | `aitk-get_evaluation_code_gen_best_practices` | Evaluation code guidance |

## Agent Creation

**When to use**: User asks to "create", "scaffold", "start", "build" a new agent or workflow.

### 1. SDK Setup

**Python**: Requires Python 3.10 or higher. Pin version (==1.0.0b260107) to avoid breaking changes like `AgentRunResponseUpdate`/`AgentResponseUpdate`, `create_agent`/`as_agent`, etc.

```bash
pip install agent-framework-azure-ai==1.0.0b260107 agent-framework-core==1.0.0b260107
```

**.NET**: Use `--prerelease`. Remind user in generated documentation.

```bash
dotnet add package Microsoft.Agents.AI.AzureAI --prerelease
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
dotnet add package Microsoft.Agents.AI.Workflows --prerelease
# or use version "*-*" for the latest version
```

### 2. Options & Alternatives

- **More Samples**: If the scenario is specific, call `githubRepo` to search for more samples before generating.
- **Minimal / Test Only**: If user requests "minimal" or "test-only" code, skip Agent-as-Server, Debug, and verification steps.
- **Deferred Config**: If user wants to configure later, skip configuration step and remind them to update `.env` later.

### 3. Creation Workflow

Use following checklist to track progress:

```markdown
Creation Progress:
- [ ] Gather context (Samples, Model, Server, Debug)
- [ ] Create implementation plan
- [ ] Select model & configure environment
- [ ] Implement code (Agent-as-Server pattern)
- [ ] Install dependencies
- [ ] Verify startup (Run-Fix loop)
- [ ] Documentation & Handoff
```

**Step 1: Gather context**

Call tools from the **Toolbelt**. For standard new agent requests:

**Required tools**:
- `aitk-get_agent_model_code_sample` - can call multiple times for different intents
- `aitk-agent_as_server` and `aitk-add_agent_debug` - for production and debug patterns
- `aitk-get_ai_model_guidance` and `aitk-list_foundry_models` - for model selection

**Recommended tools**:
- `githubRepo` - search for advanced patterns: MCP, Multimodal, Assistants API, Responses API, Copilot Studio, Anthropic, Reflection, Switch-Case, Fan-out/Fan-in, Loop, Human-in-Loop

**Step 2: Create implementation plan**

Before coding, think through a detailed step-by-step implementation plan. Output the plan (high-level steps avoiding redundant details) so user can know what you will do.

**Step 3: Select model & configure environment**

*Decide on the model BEFORE coding to ensure correct client/credential patterns.*

If user has not specified a model, transition to **Model Selection** capability.

- **Config**: Create/update `.env` (ensure not to overwrite existing variables), like:
  ```
  FOUNDRY_PROJECT_ENDPOINT=<project-endpoint>
  FOUNDRY_MODEL_DEPLOYMENT_NAME=<model-deployment-name>
  ```
- Always output what's configured, location, and how to change later if needed

**Step 4: Implement code**

- **Server Mode**: Must implement Agent-as-Server pattern (HTTP) unless "Minimal" requested.
- **Debug**: Apply dev tools and add `.vscode/launch.json` and `.vscode/tasks.json` (from `aitk-add_agent_debug`).
- **Patterns**: Use context from gather step to structure the agent or workflow.

**Step 5: Install dependencies**

1. Generate/update `requirements.txt`.
2. Check/configure Python environment: `configurePythonEnvironment`, if no env, create one.
3. Install packages: `installPythonPackage` or terminal command using correct executable.

**Step 6: Verify startup (Run-Fix loop)**

Enter a run-fix loop: run → [if unexpected error] fix → rerun → repeat until no startup/init error.

1. Run the main entrypoint (HTTP Server).
2. **If startup fails**: Fix error → Rerun.
3. **If startup succeeds**: Stop server immediately.

**Guardrails**:
- **DO** perform a real run to catch startup errors early (static syntax check is NOT enough)
- **DO** cleanup after verification. If you started the HTTP server, you MUST stop it
- **DO** ignore environment/auth/connection/timeout errors. Focus ONLY on startup/init errors
- **DON'T** wait for user input
- **DON'T** create separate test code or scripts
- **DON'T** mock configuration

**Step 7: Documentation & Handoff**

- Create/Update `README.md`.
- Remind user next steps for production-readiness:
  - Debug / F5 can help user quickly try / verify the app locally
  - Tracing setup can help monitor and troubleshoot runtime issues

## Existing Agent Enhancement

**When to use**: User asks to "update", "fix", "add feature", "enhance", or "improve" an existing agent.

**Principles** (for Microsoft Agent Framework; DO NOT change other SDKs unless explicitly asked):

1. **Context First**: Before changes, explore codebase to understand existing architecture, patterns, and dependencies.
2. **Explore & Gather**: Use **Toolbelt** to gather context while respecting existing types.
3. **Respect Tech Stack**: Do NOT migrate or change other SDKs unless explicitly requested.
4. **Respect Existing Types**: Keep existing types like `*Client`, `*Credential`, etc. No migration unless explicitly requested.
5. **New Features**: Follow **Creation Workflow** principles (Server mode, Debug support).
6. **Debug Support Addition**: Add debugging support with AI Toolkit Agent Inspector. Follow Step 6 (Verify) for correctness.
7. **Verify**: Use Step 6 from Creation Workflow to ensure no regressions.

## Specialized Capabilities

### Model Selection

**When to use**: User asks to "configure", "change", or "recommend" a model, or asks "which model" to use. Also triggered automatically during Agent Creation.

**What to do**: Use `aitk-get_ai_model_guidance` and `aitk-list_foundry_models`.
- For production-quality agents/workflows, recommend Foundry model(s).

**Important considerations**:
- User's existing model deployment is a quick start, but NOT necessarily the best choice. Recommend based on user intent and model capabilities.
- Always output clear explanation of your recommendation and show alternatives even if not deployed.
- If no Foundry project/model is available, recommend creating one via Foundry extension.

### Tracing

**When to use**: User asks to "monitor", "trace", or improve "observability".

**What to do**: Use `aitk-get_tracing_code_gen_best_practices` to retrieve best practices, then apply them to instrument the code.

### Evaluation

**When to use**: User asks to "improve performance", "measure", or "evaluate" the agent.

**What to do**:
- **Planning First**: Use `aitk-evaluation_planner` to clarify metrics, test dataset, and runtime.
- **Runner**: Use `aitk-evaluation_agent_runner_best_practices` for collecting responses from test datasets.
- **Code**: Use `aitk-get_evaluation_code_gen_best_practices` for evaluation code generation.

### Deployment

**When to use**: User asks to "deploy", "publish", or "go production" the agent.

**What to do**:
1. Ensure the app is wrapped as HTTP server (if not, use `aitk-agent_as_server` first).
2. Execute VS Code command: [Microsoft Foundry: Deploy Hosted Agent](azure-ai-foundry.commandPalette.deployWorkflow).
