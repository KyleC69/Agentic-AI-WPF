// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIChatRole.cs
// Author: Kyle L. Crowder
// Build Num: 105645



using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Models;





/// <summary>
///     Describes the intended purpose of a message within a chat interaction.
/// </summary>
[JsonConverter(typeof(Converter))]
[DebuggerDisplay("{Value,nq}")]
public readonly struct AIChatRole : IEquatable<AIChatRole>, IEquatable<ChatRole>
{
    /// <summary>Gets the role that instructs or sets the behavior of the system.</summary>
    public static AIChatRole System { get; } = new("system");

    /// <summary>Gets the role that provides responses to system-instructed, user-prompted input.</summary>
    public static AIChatRole Assistant { get; } = new("assistant");

    /// <summary>Gets the role that provides user input for chat interactions.</summary>
    public static AIChatRole User { get; } = new("user");

    /// <summary>Gets the role that provides additional information and references in response to tool use requests.</summary>
    public static AIChatRole Tool { get; } = new("tool");

    /// <summary>
    ///     Injected context from chat history or other knowledge source. This is a special role that can be used to
    ///     differentiate injected context from user/system/assistant messages.
    ///     It allows the system to treat injected context differently, such as by not including it in the conversation history
    ///     or by applying different processing rules.
    ///     NOTE: Intended for internal filtering and processing of injected context, and not necessarily for end-user display.
    ///     The value "context" is used to clearly indicate the source and purpose of these messages.
    /// </summary>
    public static AIChatRole AIContext { get; } = new("context");

    /// <summary>
    ///     Gets the value associated with this <see cref="AIChatRole" />.
    /// </summary>
    /// <remarks>
    ///     The value will be serialized into the "role" message field of the Chat Message format.
    /// </remarks>
    public string Value { get; }

    /// <summary>
    ///     Injected context from RAG or other knowledge source. This is a special role that can be used to differentiate
    ///     injected context from user/system/assistant messages.
    ///     It allows the system to treat injected context differently, such as by not including it in the conversation history
    ///     or by applying different processing rules.
    ///     NOTE: Intended for internal filtering and processing of injected context, and not necessarily for end-user display.
    ///     The value "rag_context" is used to clearly indicate the source and purpose of these messages.
    /// </summary>
    public static AIChatRole RAGContext { get; } = new("rag_context");








    /// <summary>
    ///     Initializes a new instance of the <see cref="AIChatRole" /> struct with the provided value.
    /// </summary>
    /// <param name="value">The value to associate with this <see cref="AIChatRole" />.</param>
    [JsonConstructor]
    public AIChatRole(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }








    /// <summary>
    ///     Returns a value indicating whether two <see cref="AIChatRole" /> instances are equivalent, as determined by a
    ///     case-insensitive comparison of their values.
    /// </summary>
    /// <param name="left">The first <see cref="AIChatRole" /> instance to compare.</param>
    /// <param name="right">The second <see cref="AIChatRole" /> instance to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if left and right are both <see langword="null" /> or have equivalent values;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator ==(AIChatRole left, AIChatRole right)
    {
        return left.Equals(right);
    }








    /// <summary>
    ///     Returns a value indicating whether two <see cref="AIChatRole" /> instances are not equivalent, as determined by a
    ///     case-insensitive comparison of their values.
    /// </summary>
    /// <param name="left">The first <see cref="AIChatRole" /> instance to compare. </param>
    /// <param name="right">The second <see cref="AIChatRole" /> instance to compare. </param>
    /// <returns>
    ///     <see langword="true" /> if left and right have different values; <see langword="false" /> if they have
    ///     equivalent values or are both <see langword="null" />.
    /// </returns>
    public static bool operator !=(AIChatRole left, AIChatRole right)
    {
        return !(left == right);
    }








    public static bool operator ==(AIChatRole left, ChatRole right)
    {
        return left.Equals(right);
    }








    public static bool operator !=(AIChatRole left, ChatRole right)
    {
        return !(left == right);
    }








    public static bool operator ==(ChatRole left, AIChatRole right)
    {
        return right.Equals(left);
    }








    public static bool operator !=(ChatRole left, AIChatRole right)
    {
        return !(left == right);
    }








    public static implicit operator ChatRole(AIChatRole v)
    {
        return new ChatRole(v.Value);
    }








    public static implicit operator AIChatRole(ChatRole v)
    {
        return new AIChatRole(v.Value);
    }








    /// <inheritdoc />
    public bool Equals(ChatRole other)
    {
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }








    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return (obj is AIChatRole otherRole && Equals(otherRole))
               || (obj is ChatRole chatRole && Equals(chatRole));
    }








    /// <inheritdoc />
    public bool Equals(AIChatRole other)
    {
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }








    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }








    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
















    public ChatRole ToChatRole()
    {
        return new ChatRole(Value);
    }








    public AIChatRole ToAIChatRole()
    {
        return this;
    }
}




/// <summary>Provides a <see cref="JsonConverter{AIChatRole}" /> for serializing <see cref="AIChatRole" /> instances.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class Converter : JsonConverter<AIChatRole>
{
    /// <inheritdoc />
    public override AIChatRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(reader.GetString()!);
    }








    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, AIChatRole value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
