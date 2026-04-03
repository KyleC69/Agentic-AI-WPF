// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatMessageExtensions.cs
// Author: Kyle L. Crowder
// Build Num: 095147



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Models.Extensions;





/// <summary>
///     Summary:
///     Contains extension methods for Microsoft.Extensions.AI.ChatMessage
/// </summary>
public static class ChatMessageExtensions
{

    /// <summary>
    ///     Retrieves the source ID of the specified <see cref="ChatMessage" /> in the context of messages
    ///     passed into an agent run.
    /// </summary>
    /// <param name="message">
    ///     The <see cref="ChatMessage" /> for which the source ID is to be retrieved.
    /// </param>
    /// <returns>
    ///     A <see cref="string" /> representing the source ID of the <see cref="ChatMessage" />.
    ///     Returns <c>null</c> if no explicit source ID is defined.
    /// </returns>
    public static string? GetAgentRequestMessageSourceId(this ChatMessage message)
    {
        object? value = default;
        var flag = message.AdditionalProperties?.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out value);
        return flag.HasValue && flag == true && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution ? agentRequestMessageSourceAttribution.SourceId : null;
    }








    /// <summary>
    ///     <para>
    ///         Summary:
    ///         Gets the source type of the provided Microsoft.Extensions.AI.ChatMessage in the
    ///         context of messages passed into an agent run.
    ///     </para>
    ///     <para>
    ///         Parameters:
    ///         message:
    ///         The Microsoft.Extensions.AI.ChatMessage for which we need the source type.
    ///     </para>
    ///     <para>
    ///         Returns:
    ///         An Microsoft.Agents.AI.AgentRequestMessageSourceType value indicating the source
    ///         type of the Microsoft.Extensions.AI.ChatMessage. Defaults to
    ///         Microsoft.Agents.AI.AgentRequestMessageSourceType.External
    ///         if no explicit source is defined.
    ///     </para>
    /// </summary>
    /// <param name="message">
    ///     The <see cref="ChatMessage" /> for which the source type is to be retrieved.
    /// </param>
    /// <returns>
    ///     An <see cref="AgentRequestMessageSourceType" /> value indicating the source type of the
    ///     <see cref="ChatMessage" />.
    ///     Defaults to <see cref="AgentRequestMessageSourceType.External" /> if no explicit source is defined.
    /// </returns>
    public static AgentRequestMessageSourceType GetAgentRequestMessageSourceType(this ChatMessage message)
    {
        object? value = default;
        var flag = message.AdditionalProperties?.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out value);
        return flag.HasValue && flag == true && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution ? agentRequestMessageSourceAttribution.SourceType : AgentRequestMessageSourceType.External;
    }








    /// <summary>
    ///     Tags the specified <see cref="ChatMessage" /> with the provided source type and source ID
    ///     in the context of a specific agent run.
    /// </summary>
    /// <param name="message">
    ///     The <see cref="ChatMessage" /> to tag.
    /// </param>
    /// <param name="sourceType">
    ///     The <see cref="AgentRequestMessageSourceType" /> to tag the message with.
    /// </param>
    /// <param name="sourceId">
    ///     The source ID to tag the message with. This parameter is optional and can be <c>null</c>.
    /// </param>
    /// <returns>
    ///     A new <see cref="ChatMessage" /> instance with the specified tagging applied,
    ///     or the original message if it is already tagged with the provided source type and source ID.
    /// </returns>
    /// <remarks>
    ///     If the message is already tagged with the specified source type and source ID,
    ///     the original message is returned unchanged. Otherwise, a cloned message is returned
    ///     with the appropriate tagging added to its <see cref="ChatMessage.AdditionalProperties" />.
    /// </remarks>
    public static ChatMessage WithAgentRequestMessageSource(this ChatMessage message, AgentRequestMessageSourceType sourceType, string? sourceId = null)
    {





        message = message.Clone();
        ChatMessage chatMessage = message;
        if (chatMessage.AdditionalProperties is null)
        {
            chatMessage.AdditionalProperties = [];
        }
        else
        {
            IEnumerable<KeyValuePair<string, object?>> existingProperties = chatMessage.AdditionalProperties;
            chatMessage.AdditionalProperties = [];
            foreach (var property in existingProperties) chatMessage.AdditionalProperties[property.Key] = property.Value;
        }

        _ = message.AdditionalProperties?[AgentRequestMessageSourceAttribution.AdditionalPropertiesKey] = new AgentRequestMessageSourceAttribution(sourceType, sourceId);
        return message;
    }
}