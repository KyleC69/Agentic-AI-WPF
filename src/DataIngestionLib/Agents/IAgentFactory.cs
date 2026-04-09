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



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Agents;





public interface IAgentFactory
{

    /// <summary>
    ///     Creates and returns a coding assistant agent configured with the specified parameters.
    ///     Unique agent IDs are enforced to prevent conflicts within the system. The agent is designed to assist with
    ///     diagnosing Windows operating system issues, writing C# code targeting .NET 10.0, and aiding in the development
    ///     of the application and its agent framework. It utilizes a set of tools for gathering information about the
    ///     environment, codebase, and development process, and provides troubleshooting information to help users debug
    ///     problems effectively.
    /// </summary>
    /// <param name="agentId">The unique identifier for the agent. Cannot be null.</param>
    /// <param name="model">The model to be used by the agent. Cannot be null.</param>
    /// <param name="agentDescription">An optional description of the agent.</param>
    /// <param name="instructions">
    ///     Optional instructions for the agent's behavior. If not provided, default instructions will
    ///     be used.
    /// </param>
    /// <returns>An instance of <see cref="AIAgent" /> configured as a coding assistant.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="agentId" /> or <paramref name="model" /> is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if an agent with the specified <paramref name="agentId" /> already
    ///     exists.
    /// </exception>
    AIAgent BuildAssistantAgent(IChatClient client, string agentId, string model, string agentDescription = "", string? instructions = null);



    AIAgent BuildBasicAgent(IChatClient client, string agentId, string model, string agentDescription = "", string? instructions = null);


    IEmbeddingGenerator<string, Embedding<float>> GetEmbeddingClient();


    IChatClient GetChatClient(string model);
}