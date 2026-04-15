// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         AIModelDescriptor.cs
// Author: Kyle L. Crowder
// Build Num: 194455



namespace AgentAILib.Models;





/// <summary>
///     Describes a selectable AI model with its display name and runtime model identifier.
///     Used to populate the model picker in the UI and to route requests to the correct
///     backend endpoint.
/// </summary>
/// <param name="DisplayName">Human-readable label shown in the UI.</param>
/// <param name="ModelId">The identifier string passed to the underlying client (e.g. Ollama model tag).</param>
public record AIModelDescriptor(string DisplayName, string ModelId);