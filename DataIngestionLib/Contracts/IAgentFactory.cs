using Microsoft.Agents.AI;

namespace DataIngestionLib.Contracts;

public interface IAgentFactory
{
    AIAgent GetCodingAssistantAgent();
}