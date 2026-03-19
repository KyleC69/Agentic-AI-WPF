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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;

using Microsoft.Extensions.AI;

namespace DataIngestionLib.Models;













//#
//#
//#
//#
//# Custom chat message class that can be used to represent messages in a chat conversation with an LLM. This class is designed to be flexible and extensible,
//allowing it to accommodate a wide range of use cases and underlying implementations. It includes properties for the author, role, content, and additional metadata associated with a chat message.
public class AIMessage
{

    public IList<AIContent>? _contents;
    public string? _authorName;








    // <summary>Initializes a new instance of the <see cref="ChatMessage"/> class.</summary>
    /// <remarks>The instance defaults to having a role of <see cref="ChatRole.User"/>.</remarks>
    [JsonConstructor]
    public AIMessage()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AIMessage"/> class.</summary>
    /// <param name="role">The role of the author of the message.</param>
    /// <param name="content">The text content of the message.</param>
    public AIMessage(ChatRole role, string? content)
            : this(role, content is null ? [] : [new TextContent(content)])
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ChatMessage"/> class.</summary>
    /// <param name="role">The role of the author of the message.</param>
    /// <param name="contents">The contents for this message.</param>
    public AIMessage(ChatRole role, IList<AIContent>? contents)
    {
        Role = role;
        _contents = contents;
    }

    
    /// <summary>Clones the <see cref="ChatMessage"/> to a new <see cref="ChatMessage"/> instance.</summary>
    /// <returns>A shallow clone of the original message object.</returns>
    /// <remarks>
    /// This is a shallow clone. The returned instance is different from the original, but all properties
    /// refer to the same objects as the original.
    /// </remarks>
    public ChatMessage Clone() =>
            new()
            {
                    AuthorName = _authorName,
                    AdditionalProperties = AdditionalProperties,
                    CreatedAt = CreatedAt,
                    RawRepresentation = RawRepresentation,
                    Role = Role,
                    Contents = _contents,
                    MessageId = MessageId,
            };

    /// <summary>Gets or sets the name of the author of the message.</summary>
    public string? AuthorName
    {
        get => _authorName;
        set => _authorName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>Gets or sets a timestamp for the chat message.</summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>Gets or sets the role of the author of the message.</summary>
    public ChatRole Role { get; set; } = ChatRole.User;

    /// <summary>Gets the text of this message.</summary>
    /// <remarks>
    /// This property concatenates the text of all <see cref="TextContent"/> objects in <see cref="Contents"/>.
    /// </remarks>
    [JsonIgnore]
    public string Text
    {
        get
        {
            return string.Concat(Contents.OfType<TextContent>().Select(c => c.Text));
        }
    }

    /// <summary>Gets or sets the chat message content items.</summary>
    [AllowNull]
    public IList<AIContent> Contents
    {
        get => _contents ??= [];
        set => _contents = value;
    }

    /// <summary>Gets or sets the ID of the chat message.</summary>
    public string? MessageId { get; set; }

    /// <summary>Gets or sets the raw representation of the chat message from an underlying implementation.</summary>
    /// <remarks>
    /// If a <see cref="AIMessage"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets any additional properties associated with the message.</summary>
    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Text;

    /// <summary>Gets a <see cref="AIContent"/> object to display in the debugger display.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private AIContent? ContentForDebuggerDisplay
    {
        get
        {
            string text = Text;
            return
                !string.IsNullOrWhiteSpace(text) ? new TextContent(text) :
                _contents is { Count: > 0 } ? _contents[0] :
                null;
        }
    }

    /// <summary>Gets an indication for the debugger display of whether there's more content.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string EllipsesForDebuggerDisplay => _contents is { Count: > 1 } ? ", ..." : string.Empty;

}
