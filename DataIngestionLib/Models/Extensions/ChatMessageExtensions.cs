// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatMessageExtensions.cs
// Author: Kyle L. Crowder
// Build Num: 202359



using Microsoft.Agents.AI;




namespace DataIngestionLib.Models.Extensions;





/// <summary>
///
/// Summary:
/// Contains extension methods for Microsoft.Extensions.AI.ChatMessage
/// </summary>
public static class ChatMessageExtensions
    {

    /// <summary>
    /// Retrieves the source ID of the specified <see cref="AIChatMessage"/> in the context of messages
    /// passed into an agent run.
    /// </summary>
    /// <param name="message">
    /// The <see cref="AIChatMessage"/> for which the source ID is to be retrieved.
    /// </param>
    /// <returns>
    /// A <see cref="string"/> representing the source ID of the <see cref="AIChatMessage"/>.
    /// Returns <c>null</c> if no explicit source ID is defined.
    /// </returns>
    public static string? GetAgentRequestMessageSourceId(this AIChatMessage message)
        {
        object? value = default;
        var flag = message.AdditionalProperties?.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out value);
        return flag.HasValue && flag == true && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution
                ? agentRequestMessageSourceAttribution.SourceId
                : null;
        }








    /// <summary>
    ///
    /// <para>
    /// Summary:
    /// Gets the source type of the provided Microsoft.Extensions.AI.ChatMessage in the
    /// context of messages passed into an agent run.
    /// </para>
    /// <para>
    /// Parameters:
    /// message:
    /// The Microsoft.Extensions.AI.ChatMessage for which we need the source type.
    /// </para>
    /// <para>
    /// Returns:
    /// An Microsoft.Agents.AI.AgentRequestMessageSourceType value indicating the source
    /// type of the Microsoft.Extensions.AI.ChatMessage. Defaults to Microsoft.Agents.AI.AgentRequestMessageSourceType.External
    /// if no explicit source is defined.
    /// </para>
    /// </summary>
    /// <param name="message">
    /// The <see cref="AIChatMessage"/> for which the source type is to be retrieved.
    /// </param>
    /// <returns>
    /// An <see cref="AgentRequestMessageSourceType"/> value indicating the source type of the <see cref="AIChatMessage"/>.
    /// Defaults to <see cref="AgentRequestMessageSourceType.External"/> if no explicit source is defined.
    /// </returns>
    public static AgentRequestMessageSourceType GetAgentRequestMessageSourceType(this AIChatMessage message)
        {
        object? value = default;
        var flag = message.AdditionalProperties?.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out value);
        return flag.HasValue && flag == true && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution
                ? agentRequestMessageSourceAttribution.SourceType
                : AgentRequestMessageSourceType.External;
        }








    /// <summary>
    /// Tags the specified <see cref="AIChatMessage"/> with the provided source type and source ID
    /// in the context of a specific agent run.
    /// </summary>
    /// <param name="message">
    /// The <see cref="AIChatMessage"/> to tag.
    /// </param>
    /// <param name="sourceType">
    /// The <see cref="AgentRequestMessageSourceType"/> to tag the message with.
    /// </param>
    /// <param name="sourceId">
    /// The source ID to tag the message with. This parameter is optional and can be <c>null</c>.
    /// </param>
    /// <returns>
    /// A new <see cref="AIChatMessage"/> instance with the specified tagging applied,
    /// or the original message if it is already tagged with the provided source type and source ID.
    /// </returns>
    /// <remarks>
    /// If the message is already tagged with the specified source type and source ID,
    /// the original message is returned unchanged. Otherwise, a cloned message is returned
    /// with the appropriate tagging added to its <see cref="AIChatMessage.AdditionalProperties"/>.
    /// </remarks>
    public static AIChatMessage WithAgentRequestMessageSource(this AIChatMessage message, AgentRequestMessageSourceType sourceType, string? sourceId = null)
        {
        if (message.AdditionalProperties != null && message.AdditionalProperties.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out var value) && value is AgentRequestMessageSourceAttribution agentRequestMessageSourceAttribution && agentRequestMessageSourceAttribution.SourceType == sourceType && agentRequestMessageSourceAttribution.SourceId == sourceId)
            {
            return message;
            }

        message = message.Clone();
        AIChatMessage chatMessage = message;
        if (chatMessage.AdditionalProperties is null)
            {
            chatMessage.AdditionalProperties = [];
            }
        else
            {
            IEnumerable<KeyValuePair<string, object?>> existingProperties = chatMessage.AdditionalProperties;
            chatMessage.AdditionalProperties = [];
            foreach (KeyValuePair<string, object?> property in existingProperties)
                {
                chatMessage.AdditionalProperties[property.Key] = property.Value;
                }
            }

        message.AdditionalProperties?[AgentRequestMessageSourceAttribution.AdditionalPropertiesKey] = new AgentRequestMessageSourceAttribution(sourceType, sourceId);
        return message;
        }
    }