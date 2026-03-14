// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIChatMessage.cs
// Author: Kyle L. Crowder
// Build Num: 175054



using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.AI;

using Newtonsoft.Json;

using AIContent = Microsoft.Extensions.AI.AIContent;
using TextContent = Microsoft.Extensions.AI.TextContent;




namespace DataIngestionLib.Models;





/// <summary>Represents a chat message used by an <see cref="IChatClient" />.</summary>
/// EXTENDED CHAT MESSAGE CLASS TO NORMALIZE DB TIMESTAMPS
/// Other enhancers added to use for UI binding directly
/// <related type="Article" href="https://learn.microsoft.com/dotnet/ai/quickstarts/build-chat-app">
///     Build an AI chat app
///     with .NET.
/// </related>
[DebuggerDisplay("[{Role}] {ContentForDebuggerDisplay}{EllipsesForDebuggerDisplay,nq}")]
public class AIChatMessage : IEquatable<AIChatMessage>
{
    private string? _authorName;
    private IList<AIContent>? _contents;








    /// <summary>Initializes a new instance of the <see cref="AIChatMessage" /> class.</summary>
    /// <remarks>The instance defaults to having a role of <see cref="ChatRole.User" />.</remarks>
    [JsonConstructor]
    public AIChatMessage()
    {
    }








    public AIChatMessage(AIChatRole role, string? content)
            : this(role, content is null ? [] : [new TextContent(content)])
    {
    }








    /// <summary>Initializes a new instance of the <see cref="AIChatMessage" /> class.</summary>
    /// <param name="role">The role of the author of the message.</param>
    /// <param name="content">The text content of the message.</param>
    public AIChatMessage(ChatRole role, string? content)
            : this(role, content is null ? [] : [new TextContent(content)])
    {
    }








    /// <summary>Initializes a new instance of the <see cref="AIChatMessage" /> class.</summary>
    /// <param name="role">The role of the author of the message.</param>
    /// <param name="contents">The contents for this message.</param>
    public AIChatMessage(ChatRole role, IList<AIContent>? contents)
    {
        Role = role;
        _contents = contents;
    }








    public AIChatMessage(AIChatRole role, IList<AIContent>? contents)
    {
        Role = role;
        _contents = contents;
    }








    /// <summary>Gets or sets any additional properties associated with the message.</summary>
    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }





    /// <summary>Gets or sets the name of the author of the message.</summary>
    public string? AuthorName
    {
        get { return _authorName; }
        set { _authorName = string.IsNullOrWhiteSpace(value) ? null : value; }
    }





    /// <summary>Gets a <see cref="AIContent" /> object to display in the debugger display.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private AIContent? ContentForDebuggerDisplay
    {
        get
        {
            var text = Text;
            return
                    !string.IsNullOrWhiteSpace(text) ? new TextContent(text) :
                    _contents is { Count: > 0 } ? _contents[0] :
                    null;
        }
    }





    /// <summary>Gets or sets the chat message content items.</summary>
    [AllowNull]
    public IList<AIContent> Contents
    {
        get { return _contents ??= []; }
        set { _contents = value; }
    }





    /// <summary>Gets or sets a timestamp for the chat message.</summary>
    public DateTimeOffset? CreatedAt { get; set; }





    /// <summary>Gets an indication for the debugger display of whether there's more content.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string EllipsesForDebuggerDisplay
    {
        get { return _contents is { Count: > 1 } ? ", ..." : string.Empty; }
    }





    /// <summary>
    ///     Gets a value indicating whether the chat message was authored by a user.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the role of the author is <see cref="ChatRole.User" />; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is useful for distinguishing messages authored by users from those authored by other roles.
    /// </remarks>
    public bool IsUser
    {
        get { return Role == AIChatRole.User; }
    }





    /// <summary>Gets or sets the ID of the chat message.</summary>
    public string? MessageId { get; set; }

    /// <summary>Gets or sets the raw representation of the chat message from an underlying implementation.</summary>
    /// <remarks>
    ///     If a <see cref="AIChatMessage" /> is created to represent some underlying object from another object
    ///     model, this property can be used to store that original object. This can be useful for debugging or
    ///     for enabling a consumer to access the underlying object model if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets the role of the author of the message.</summary>
    public AIChatRole Role { get; set; } = AIChatRole.User;





    /// <summary>Gets the text of this message.</summary>
    /// <remarks>
    ///     This property concatenates the text of all <see cref="TextContent" /> objects in <see cref="Contents" />.
    /// </remarks>
    [JsonIgnore]
    public string Text
    {
        get
        {

            var vb = string.Concat(Contents.OfType<TextContent>().Select(c => c.Text));
            return vb;
        }
    }





    /// <summary>Gets a timestamp for the chat message normalized to the local time zone.</summary>
    public DateTimeOffset TimeStampOffset
    {
        get { return DateTime.Now; }
    }








    /// <inheritdoc />
    public bool Equals(AIChatMessage? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (Role == other.Role
                                                                      && string.Equals(Text, other.Text, StringComparison.Ordinal)
                                                                      && string.Equals(AuthorName, other.AuthorName, StringComparison.Ordinal)
                                                                      && Nullable.Equals(CreatedAt, other.CreatedAt)
                                                                      && string.Equals(MessageId, other.MessageId, StringComparison.Ordinal)));
    }








    /// <summary>Clones the <see cref="AIChatMessage" /> to a new <see cref="AIChatMessage" /> instance.</summary>
    /// <returns>A shallow clone of the original message object.</returns>
    /// <remarks>
    ///     This is a shallow clone. The returned instance is different from the original, but all properties
    ///     refer to the same objects as the original.
    /// </remarks>
    public AIChatMessage Clone()
    {
        return new()
        {
                AdditionalProperties = AdditionalProperties,
                _authorName = _authorName,
                _contents = _contents,
                CreatedAt = CreatedAt,
                RawRepresentation = RawRepresentation,
                Role = Role,
                MessageId = MessageId
        };
    }








    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is AIChatMessage other && Equals(other);
    }








    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Role, Text, AuthorName, CreatedAt, MessageId);
    }








    public static bool operator ==(AIChatMessage? left, AIChatMessage? right)
    {
        return ReferenceEquals(left, right) || (left is not null && right is not null && left.Equals(right));
    }








    public static bool operator !=(AIChatMessage? left, AIChatMessage? right)
    {
        return !(left == right);
    }








    /// <inheritdoc />
    public override string ToString()
    {
        return Text ?? string.Empty;
    }
}