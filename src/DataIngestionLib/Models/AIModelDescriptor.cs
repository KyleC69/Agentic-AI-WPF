// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



namespace AgentAILib.Models;




/// <summary>
///     Describes a selectable AI model with its display name and runtime model identifier.
///     Used to populate the model picker in the UI and to route requests to the correct
///     backend endpoint.
/// </summary>
/// <param name="DisplayName">Human-readable label shown in the UI.</param>
/// <param name="ModelId">The identifier string passed to the underlying client (e.g. Ollama model tag).</param>
public record AIModelDescriptor(string DisplayName, string ModelId);
