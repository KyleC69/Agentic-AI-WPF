// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         FixedAgentIdentityProvider.cs
// Author: Kyle L. Crowder
// Build Num: 105637



using DataIngestionLib.Contracts.Services;




namespace DataIngestionLib.Services;





/// <summary>
///     An <see cref="IAgentIdentityProvider" /> implementation that returns a fixed, pre-configured
///     agent identifier.
/// </summary>
/// <remarks>
///     Use this implementation when the agent identity is known at startup and does not change
///     during the lifetime of the application. Register it in the DI container with the desired
///     agent ID string, for example:
///     <code>
///     services.AddSingleton&lt;IAgentIdentityProvider&gt;(
///         new FixedAgentIdentityProvider("coding-assistant"));
///     </code>
/// </remarks>
public sealed class FixedAgentIdentityProvider : IAgentIdentityProvider
{
    private readonly string _agentId;








    /// <summary>
    ///     Initializes a new instance of <see cref="FixedAgentIdentityProvider" /> with the supplied
    ///     agent identifier.
    /// </summary>
    /// <param name="agentId">
    ///     A non-null, non-whitespace string that uniquely identifies the agent within the application.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="agentId" /> is null, empty, or consists only of white-space
    ///     characters.
    /// </exception>
    public FixedAgentIdentityProvider(string agentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        _agentId = agentId;
    }








    /// <inheritdoc />
    public string GetAgentId() => _agentId;
}