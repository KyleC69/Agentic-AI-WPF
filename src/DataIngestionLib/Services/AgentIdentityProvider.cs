// Build Date: 2026/03/20
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AgentIdentityProvider.cs

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;

namespace DataIngestionLib.Services;

/// <summary>
///     Resolves the configured AI agent identity for session scoping and history attribution.
/// </summary>
public sealed class AgentIdentityProvider : IAgentIdentityProvider
{
    private const string DefaultAgentId = "Agentic-Max";

    private readonly IAppSettings _appSettings;

    public AgentIdentityProvider(IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        _appSettings = appSettings;
    }

    /// <inheritdoc />
    public string GetAgentId()
    {
        string configuredAgentId = _appSettings.AgentId?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(configuredAgentId) ? DefaultAgentId : configuredAgentId;
    }
}