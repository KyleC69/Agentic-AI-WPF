// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ISqlVectorStore.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Contracts.Services;





public interface ISqlVectorStore
{


    /// <summary>
    ///     Retrieves persisted chat history for a user in ascending timestamp order.
    /// </summary>
    /// <param name="userId">Application user identifier.</param>
    /// <returns>Ordered chat message and timestamp tuples.</returns>
    IEnumerable<(string Message, DateTime Timestamp)> GetChatHistory(string userId);








    /// <summary>
    ///     Persists a chat message for a user.
    /// </summary>
    /// <param name="userId">Application user identifier.</param>
    /// <param name="message">Message content to persist.</param>
    /// <param name="timestamp">Message timestamp. Values are normalized to UTC before persistence.</param>
    void SaveChatHistory(string userId, string message, DateTime timestamp);
}