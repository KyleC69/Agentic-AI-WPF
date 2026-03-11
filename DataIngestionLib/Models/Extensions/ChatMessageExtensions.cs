// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatMessageExtensions.cs
//   Author: Kyle L. Crowder



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Models.Extensions;





//
// Summary:
//     Contains extension methods for Microsoft.Extensions.AI.ChatMessage
public static class ChatMessageExtensions
{

    //
    // Summary:
    //     Gets the source id of the provided Microsoft.Extensions.AI.ChatMessage in the
    //     context of messages passed into an agent run.
    //
    // Parameters:
    //   message:
    //     The Microsoft.Extensions.AI.ChatMessage for which we need the source id.
    //
    // Returns:
    //     An System.String value indicating the source id of the Microsoft.Extensions.AI.ChatMessage.
    //     Defaults to null if no explicit source id is defined.
    public static string? GetAgentRequestMessageSourceId(this AIChatMessage message)
    {
        object? value = default;
        var flag = message.AdditionalProperties?.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out value);
        return flag.HasValue && flag == true && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution
                ? agentRequestMessageSourceAttribution.SourceId
                : null;
    }








    //
    // Summary:
    //     Gets the source type of the provided Microsoft.Extensions.AI.ChatMessage in the
    //     context of messages passed into an agent run.
    //
    // Parameters:
    //   message:
    //     The Microsoft.Extensions.AI.ChatMessage for which we need the source type.
    //
    // Returns:
    //     An Microsoft.Agents.AI.AgentRequestMessageSourceType value indicating the source
    //     type of the Microsoft.Extensions.AI.ChatMessage. Defaults to Microsoft.Agents.AI.AgentRequestMessageSourceType.External
    //     if no explicit source is defined.
    public static AgentRequestMessageSourceType GetAgentRequestMessageSourceType(this AIChatMessage message)
    {
        object? value = default;
        var flag = message.AdditionalProperties?.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out value);
        return flag.HasValue && flag == true && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution
                ? agentRequestMessageSourceAttribution.SourceType
                : AgentRequestMessageSourceType.External;
    }








    //
    // Summary:
    //     Ensure that the provided message is tagged with the provided source type and
    //     source id in the context of a specific agent run.
    //
    // Parameters:
    //   message:
    //     The message to tag.
    //
    //   sourceType:
    //     The source type to tag the message with.
    //
    //   sourceId:
    //     The source id to tag the message with.
    //
    // Returns:
    //     The tagged message.
    //
    // Remarks:
    //     If the message is already tagged with the provided source type and source id,
    //     it is returned as is. Otherwise, a cloned message is returned with the appropriate
    //     tagging in the AdditionalProperties.
    public static AIChatMessage WithAgentRequestMessageSource(this AIChatMessage message, AgentRequestMessageSourceType sourceType, string? sourceId = null)
    {
        if (message.AdditionalProperties != null && message.AdditionalProperties.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out var value) && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution && agentRequestMessageSourceAttribution.SourceType == sourceType && agentRequestMessageSourceAttribution.SourceId == sourceId)
        {
            return message;
        }

        message = message.Clone();
        AIChatMessage chatMessage = message;
        if (chatMessage.AdditionalProperties == null)
        {
            AdditionalPropertiesDictionary additionalPropertiesDictionary = chatMessage.AdditionalProperties = [];
        }

        message.AdditionalProperties[AgentRequestMessageSourceAttribution.AdditionalPropertiesKey] = new AgentRequestMessageSourceAttribution(sourceType, sourceId);
        return message;
    }
}