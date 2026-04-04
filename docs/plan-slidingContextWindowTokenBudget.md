## Plan: Sliding Context Window + Running Token Budget Tracking

Implement a token-based sliding window in the active history pipeline so oldest messages drop as needed, with no summarization or compaction, while adding a conversation-scoped running token calculator that tracks usage by purpose and evaluates against `TokenBudget` as warning-only.

**Steps**
1. Phase 1: Lock active architecture and exclude stale paths.  
Use only the currently wired path in [src/DataIngestionLib/Agents/AgentFactory.cs](src/DataIngestionLib/Agents/AgentFactory.cs) and [src/DataIngestionLib/Providers/SqlChatHistoryProvider.cs](src/DataIngestionLib/Providers/SqlChatHistoryProvider.cs). Explicitly ignore abandoned/commented attempts and absent planned abstractions in decision-making.
2. Phase 2: Implement token-based sliding window in the history provider.  
Add deterministic window selection in [src/DataIngestionLib/Providers/SqlChatHistoryProvider.cs](src/DataIngestionLib/Providers/SqlChatHistoryProvider.cs) with these rules: keep newest, drop oldest, always keep system messages, always keep latest user, always keep latest assistant, and return final messages in chronological order.
3. Phase 3: Add running token usage calculator as authoritative service state.  
In [src/DataIngestionLib/Services/ChatConversationService.cs](src/DataIngestionLib/Services/ChatConversationService.cs), maintain up-to-date counters by purpose (session/system/rag/tool plus total/input/output/cached/reasoning), updated on pre-request context and post-response usage.
4. Phase 4: Enforce TokenBudget as warning policy.  
Use [src/DataIngestionLib/Services/Contracts/TokenBudget.cs](src/DataIngestionLib/Services/Contracts/TokenBudget.cs) to evaluate per-purpose and global limits (`SessionBudget`, `SystemBudget`, `RAGBudget`, `ToolBudget`, `MetaBudget`, `MaximumContext`, `BudgetTotal`) and emit warnings only, never block execution.
5. Phase 5: Align service/UI contract for token telemetry.  
Expose token snapshot updates from [src/DataIngestionLib/Contracts/Services/IChatConversationService.cs](src/DataIngestionLib/Contracts/Services/IChatConversationService.cs), then bind [src/RAGDataIngestionWPF/ViewModels/MainViewModel.cs](src/RAGDataIngestionWPF/ViewModels/MainViewModel.cs) to service updates instead of relying on static middleware events as the primary source of truth.
6. Phase 6: Unify runtime settings source for context limits.  
Wire runtime context-window ceiling to a single settings source via [src/DataIngestionLib/AppSettings.cs](src/DataIngestionLib/AppSettings.cs) and existing settings plumbing, preventing drift between UI and runtime limits.
7. Phase 7: Add deterministic tests and end-to-end verification.  
Add MSTest coverage in [tests/RAGDataIngestionWPF.Tests.MSTest](tests/RAGDataIngestionWPF.Tests.MSTest) for sliding-window selection, must-keep rules, purpose attribution, and warning behavior when budgets are exceeded.

**Relevant files**
- [src/DataIngestionLib/Providers/SqlChatHistoryProvider.cs](src/DataIngestionLib/Providers/SqlChatHistoryProvider.cs) — sliding window selection logic.
- [src/DataIngestionLib/Services/ChatConversationService.cs](src/DataIngestionLib/Services/ChatConversationService.cs) — running token state + warning emission.
- [src/DataIngestionLib/Contracts/Services/IChatConversationService.cs](src/DataIngestionLib/Contracts/Services/IChatConversationService.cs) — token update/event contract.
- [src/DataIngestionLib/Services/Contracts/TokenBudget.cs](src/DataIngestionLib/Services/Contracts/TokenBudget.cs) — budget limits by purpose.
- [src/DataIngestionLib/Agents/TokenAccountingMiddleware.cs](src/DataIngestionLib/Agents/TokenAccountingMiddleware.cs) — usage normalization/category logic to reuse.
- [src/DataIngestionLib/Agents/AgentFactory.cs](src/DataIngestionLib/Agents/AgentFactory.cs) — confirm provider/middleware wiring.
- [src/DataIngestionLib/AppSettings.cs](src/DataIngestionLib/AppSettings.cs) — runtime config source for max context.
- [src/RAGDataIngestionWPF/ViewModels/MainViewModel.cs](src/RAGDataIngestionWPF/ViewModels/MainViewModel.cs) — UI token counters fed by service.
- [tests/RAGDataIngestionWPF.Tests.MSTest](tests/RAGDataIngestionWPF.Tests.MSTest) — new/updated unit and integration tests.

**Verification**
1. Build library: `dotnet build src/DataIngestionLib/DataIngestionLib.csproj`
2. Build app: `dotnet build src/RAGDataIngestionWPF/RAGDataIngestionWPF.csproj`
3. Run tests: `dotnet test tests/RAGDataIngestionWPF.Tests.MSTest/RAGDataIngestionWPF.Tests.MSTest.csproj`
4. Manual run: verify old messages slide out under token pressure while must-keep messages remain, and budget exceedances produce warnings without blocking responses.

**Decisions captured**
1. Window type: token-count-based.
2. Eviction strategy: newest retained, oldest removed.
3. Must-keep: all system messages, latest user, latest assistant.
4. Budget behavior: warn only.
5. No summarization and no compaction.
6. Stale prior attempts are explicitly excluded from design decisions.

If you approve this plan, I can hand it off for implementation exactly as scoped.
