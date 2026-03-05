using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Contracts.Services;

public interface IUserDataService
{
    event EventHandler<UserViewModel> UserDataUpdated;

    void Initialize();

    UserViewModel GetUser();
}
