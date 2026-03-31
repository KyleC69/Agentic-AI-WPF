// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Num: 233158



using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using RAGDataIngestionWPF.Core.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





internal sealed class IdentityCacheService : IIdentityCacheService
{
    private readonly Lock _fileLock = new();
    public static readonly string MsalCacheFileName = ".msalcache.bin3";
    public static readonly string MsalCacheFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{Assembly.GetExecutingAssembly().GetName().Name}";








    public byte[] ReadMsalToken()
    {
        lock (_fileLock)
        {
            var filePath = Path.Combine(MsalCacheFilePath, MsalCacheFileName);
            if (File.Exists(filePath))
            {
                var encryptedData = File.ReadAllBytes(filePath);
                return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            }

            return default;
        }
    }








    public void SaveMsalToken(byte[] token)
    {
        ArgumentNullException.ThrowIfNull(token);
        lock (_fileLock)
        {
            if (!Directory.Exists(MsalCacheFilePath))
            {
                _ = Directory.CreateDirectory(MsalCacheFilePath);
            }

            var encryptedData = ProtectedData.Protect(token, null, DataProtectionScope.CurrentUser);
            var filePath = Path.Combine(MsalCacheFilePath, MsalCacheFileName);
            File.WriteAllBytes(filePath, encryptedData);
        }
    }
}