using RAGDataIngestionWPF.Core.Models;

namespace RAGDataIngestionWPF.Core.Contracts.Services;

public interface IMicrosoftGraphService
{
    Task<User> GetUserInfoAsync(string accessToken);

    Task<string> GetUserPhoto(string accessToken);
}
