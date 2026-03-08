using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace DataIngestionLib.Models;

/// <summary>
/// Describes the intended purpose of a message within a chat interaction.
/// </summary>
[JsonConverter(typeof(Converter))]
[DebuggerDisplay("{Value,nq}")]
public readonly struct AIChatRole : IEquatable<AIChatRole>
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
    /// Gets the role that represents additional context that has been added to the conversation, such as RAG, ChatHistory, External Knowledge etc.
    /// </summary>
    public static AIChatRole AIContext { get; } = new("context");

    /// <summary>
    /// Gets the value associated with this <see cref="AIChatRole"/>.
    /// </summary>
    /// <remarks>
    /// The value will be serialized into the "role" message field of the Chat Message format.
    /// </remarks>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIChatRole"/> struct with the provided value.
    /// </summary>
    /// <param name="value">The value to associate with this <see cref="AIChatRole"/>.</param>
    [JsonConstructor]
    public AIChatRole(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>
    /// Returns a value indicating whether two <see cref="AIChatRole"/> instances are equivalent, as determined by a
    /// case-insensitive comparison of their values.
    /// </summary>
    /// <param name="left">The first <see cref="AIChatRole"/> instance to compare.</param>
    /// <param name="right">The second <see cref="AIChatRole"/> instance to compare.</param>
    /// <returns><see langword="true"/> if left and right are both <see langword="null"/> or have equivalent values; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AIChatRole left, AIChatRole right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns a value indicating whether two <see cref="AIChatRole"/> instances are not equivalent, as determined by a
    /// case-insensitive comparison of their values.
    /// </summary>
    /// <param name="left">The first <see cref="AIChatRole"/> instance to compare. </param>
    /// <param name="right">The second <see cref="AIChatRole"/> instance to compare. </param>
    /// <returns><see langword="true"/> if left and right have different values; <see langword="false"/> if they have equivalent values or are both <see langword="null"/>.</returns>
    public static bool operator !=(AIChatRole left, AIChatRole right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is AIChatRole otherRole && Equals(otherRole);
    }

    /// <inheritdoc/>
    public bool Equals(AIChatRole other)
    {
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>Provides a <see cref="JsonConverter{AIChatRole}"/> for serializing <see cref="AIChatRole"/> instances.</summary>
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
}
