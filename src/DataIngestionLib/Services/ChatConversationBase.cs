// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationBase.cs
// Author: Kyle L. Crowder
// Build Num: 212915



using DataIngestionLib.Agents;
using DataIngestionLib.Contracts;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Services;





public class ChatConversationBase
{
    protected AIAgent? _agent;
    protected IAgentFactory? _agentFactory;
    protected AgentSession? _agentSession;
    protected IHistoryIdentityService? _historyIdentityService;
    protected readonly SemaphoreSlim _initializeGate = new(1, 1);
    protected string? _initialUserId;
    protected ILogger<ChatConversationService>? _logger;
    protected RagDataService? _ragDataService;
    protected ProviderSessionState<HistoryIdentity> _sessionStateHelper = null!;
    protected const string DefaultAgentId = "Agentic-Max";

    /// <summary>
    ///     This is to provide an identifier in enterprise scenarios running multiple applications.
    /// </summary>
    public string? ApplicationId { get; set; }

    /// <summary>
    ///     A collection of settings to provide the token budget allocated for the model.
    /// </summary>
    private TokenBudget? ConversationTokenBudget { get; }

    /// <summary>
    ///     The last conversation ID that was set. The WPF layer should read this property
    ///     and persist it to Settings. Default.LastConversationId.
    /// </summary>
    public string LastConversationIdValue { get; private set; } = string.Empty;

    protected AppSettings Settings { get; } = new AppSettings();








    private static int ClampToInt(long value)
    {
        return value <= 0 ? 0 : value >= int.MaxValue ? int.MaxValue : (int)value;
    }








    private static int ReadInt(IReadOnlyDictionary<string, long> values, string key, int fallback)
    {
        return values.TryGetValue(key, out var value) ? ClampToInt(value) : fallback;
    }
}