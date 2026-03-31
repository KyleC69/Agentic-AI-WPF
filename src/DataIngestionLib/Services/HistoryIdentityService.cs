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



using System.IO;

using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





/// <summary>
///     For the centralized management of the ConversationIdentity across the conversation.
/// </summary>
public sealed class HistoryIdentityService : IHistoryIdentityService, IAgentIdentityProvider
{
    private readonly HistoryIdentity _current = new();
    private readonly object _syncLock = new();








    public string GetAgentId()
    {
        HistoryIdentity snapshot = Current;
        return string.IsNullOrWhiteSpace(snapshot.AgentId) ? "unknown-agent" : snapshot.AgentId;
    }








    public HistoryIdentity Current
    {
        get
        {
            lock (_syncLock)
            {
                return new HistoryIdentity { AgentId = _current.AgentId, ApplicationId = _current.ApplicationId, ConversationId = _current.ConversationId, UserId = _current.UserId };
            }
        }
    }








    public void Initialize(string applicationId, string agentId, string userId)
    {
        Guard.IsNotNullOrEmpty(applicationId);
        Guard.IsNotNullOrEmpty(agentId);

        var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? Environment.UserName : userId.Trim();
        Guard.IsNotNullOrEmpty(normalizedUserId, nameof(userId));
        lock (_syncLock)
        {
            _current.ApplicationId = applicationId.Trim();
            _current.AgentId = agentId.Trim();
            _current.UserId = normalizedUserId;
            _current.ConversationId = GetConversationId();
        }
    }














    public void ApplyToSession(AgentSession session)
    {
        Guard.IsNotNull(session);

        HistoryIdentity snapshot = Current;

        session.StateBag.SetValue("ApplicationId", snapshot.ApplicationId);
        session.StateBag.SetValue("AgentId", snapshot.AgentId);
        session.StateBag.SetValue("UserId", snapshot.UserId);
        session.StateBag.SetValue("UserName", snapshot.UserId);

        if (!string.IsNullOrWhiteSpace(snapshot.ConversationId))
        {
            session.StateBag.SetValue("ConversationId", snapshot.ConversationId);
        }
    }







    /// <summary>
    ///     Retrieves the conversation ID from a local file.
    /// </summary>
    /// <remarks>
    ///     This method reads the conversation ID stored in a file located in the local application data folder.
    ///     The file is expected to be named "conversationid.txt".
    /// </remarks>
    /// <returns>
    ///     The conversation ID as a <see cref="string"/> read from the file.
    /// </returns>
    /// <exception cref="IOException">
    ///     Thrown if an I/O error occurs while accessing the file.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Thrown if the application does not have the required permissions to access the file.
    /// </exception>
    internal static string GetConversationId()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(path, "conversationid.txt");
        try
        {
            if (!new FileInfo(filePath).Exists)
            {
                var id = Guid.NewGuid().ToString("N");
                File.WriteAllText(filePath, id);
                return id;
            }

            return File.ReadAllText(filePath);
        }
        catch (Exception)
        {
            throw;
        }
    }








    /// <summary>
    ///     Saves the specified conversation ID to a local file for persistence.
    /// </summary>
    /// <param name="conversationId">
    ///     The conversation ID to be saved. If <c>null</c>, a new conversation ID will be generated if <paramref name="createNew"/> is <c>true</c>.
    /// </param>
    /// <param name="createNew">
    ///     A boolean value indicating whether to generate a new conversation ID if <paramref name="conversationId"/> is <c>null</c>.
    ///     Defaults to <c>false</c>.
    /// </param>
    /// <remarks>
    ///     The conversation ID is saved to a file named "conversationid.txt" located in the local application data folder.
    /// </remarks>
    /// <exception cref="IOException">
    ///     Thrown if an I/O error occurs while writing to the file.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Thrown if the application does not have the required permissions to write to the file.
    /// </exception>
    internal static void SaveConversationId(string? conversationId, bool createNew = false)
    {
        if (createNew)
        {
            conversationId = Guid.NewGuid().ToString("N");
        }

        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(path, "conversationid.txt");
        File.WriteAllText(filePath, conversationId);
    }
}