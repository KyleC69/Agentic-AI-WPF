// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         IdentityCacheService.cs
//   Author: Kyle L. Crowder



using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using RAGDataIngestionWPF.Core.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public class IdentityCacheService : IIdentityCacheService
{
    private readonly object _fileLock = new();
    public static readonly string _msalCacheFileName = ".msalcache.bin3";
    public static readonly string _msalCacheFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{Assembly.GetExecutingAssembly().GetName().Name}";








    public byte[] ReadMsalToken()
    {
        lock (_fileLock)
        {
            string filePath = Path.Combine(_msalCacheFilePath, _msalCacheFileName);
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            }

            return default;
        }
    }








    public void SaveMsalToken(byte[] token)
    {
        lock (_fileLock)
        {
            if (!Directory.Exists(_msalCacheFilePath))
            {
                Directory.CreateDirectory(_msalCacheFilePath);
            }

            byte[] encryptedData = ProtectedData.Protect(token, null, DataProtectionScope.CurrentUser);
            string filePath = Path.Combine(_msalCacheFilePath, _msalCacheFileName);
            File.WriteAllBytes(filePath, encryptedData);
        }
    }
}