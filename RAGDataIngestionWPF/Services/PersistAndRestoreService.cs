// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         PersistAndRestoreService.cs
//   Author: Kyle L. Crowder



using System.Collections;
using System.IO;

using Microsoft.Extensions.Options;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.Services;





public class PersistAndRestoreService : IPersistAndRestoreService
{
    private readonly AppSettings _appConfig;
    private readonly IFileService _fileService;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);








    public PersistAndRestoreService(IFileService fileService, IOptions<AppSettings> appConfig)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
    }








    public void PersistData()
    {
        if (App.Current.Properties != null)
        {
            var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
            var fileName = _appConfig.AppPropertiesFileName;
            _fileService.Save(folderPath, fileName, App.Current.Properties);
        }
    }








    public void RestoreData()
    {
        var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
        var fileName = _appConfig.AppPropertiesFileName;
        IDictionary properties = _fileService.Read<IDictionary>(folderPath, fileName);
        if (properties != null)
        {
            foreach (DictionaryEntry property in properties) App.Current.Properties.Add(property.Key, property.Value);
        }
    }
}