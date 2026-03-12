// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         UserDataService.cs
// Author: Kyle L. Crowder
// Build Num: 105615



using System.IO;
using System.Windows.Media.Imaging;

using Microsoft.Extensions.Options;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Models;
using RAGDataIngestionWPF.Helpers;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Services;





public sealed class UserDataService : IUserDataService
{
    private readonly AppSettings _appConfig;
    private readonly IFileService _fileService;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private UserViewModel _user;








    public UserDataService(IFileService fileService, IOptions<AppSettings> appConfig)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
    }










    public void Initialize()
    {
    }








    public UserViewModel GetUser()
    {
        _user ??= GetUserFromCache() ?? GetDefaultUserData();

        return _user;
    }








    private UserViewModel GetDefaultUserData()
    {
        return new UserViewModel
        {
            Name = Environment.UserName,
            Photo = ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
        };
    }








    private UserViewModel GetUserFromCache()
    {
        string folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
        string fileName = _appConfig.UserFileName;
        User cacheData = _fileService.Read<User>(folderPath, fileName);
        return GetUserViewModelFromData(cacheData);
    }








    private UserViewModel GetUserViewModelFromData(User userData)
    {
        if (userData == null)
        {
            return null;
        }

        BitmapImage userPhoto = string.IsNullOrEmpty(userData.Photo)
                ? ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
                : ImageHelper.ImageFromString(userData.Photo);

        return new UserViewModel
        {
            Name = userData.DisplayName,
            UserPrincipalName = userData.UserPrincipalName,
            Photo = userPhoto
        };
    }
}