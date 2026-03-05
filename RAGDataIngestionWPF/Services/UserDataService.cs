using System.IO;

using Microsoft.Extensions.Options;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Models;
using RAGDataIngestionWPF.Helpers;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Services;

public class UserDataService : IUserDataService
{
    private readonly IFileService _fileService;
    private readonly AppConfig _appConfig;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private UserViewModel _user;

    public event EventHandler<UserViewModel> UserDataUpdated;

    public UserDataService(IFileService fileService, IOptions<AppConfig> appConfig)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
    }

    public void Initialize()
    {
    }

    public UserViewModel GetUser()
    {
        if (_user == null)
        {
            _user = GetUserFromCache();
            if (_user == null)
            {
                _user = GetDefaultUserData();
            }
        }

        return _user;
    }

    private UserViewModel GetUserFromCache()
    {
        var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
        var fileName = _appConfig.UserFileName;
        var cacheData = _fileService.Read<User>(folderPath, fileName);
        return GetUserViewModelFromData(cacheData);
    }

    private UserViewModel GetUserViewModelFromData(User userData)
    {
        if (userData == null)
        {
            return null;
        }

        var userPhoto = string.IsNullOrEmpty(userData.Photo)
            ? ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
            : ImageHelper.ImageFromString(userData.Photo);

        return new UserViewModel()
        {
            Name = userData.DisplayName,
            UserPrincipalName = userData.UserPrincipalName,
            Photo = userPhoto
        };
    }

    private UserViewModel GetDefaultUserData()
    {
        return new UserViewModel()
        {
            Name = Environment.UserName,
            Photo = ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
        };
    }
}
