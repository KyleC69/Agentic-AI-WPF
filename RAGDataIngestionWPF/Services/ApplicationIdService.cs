// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         ApplicationIdService.cs
//   Author: Kyle L. Crowder



using System.Reflection;

using Microsoft.Win32;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public sealed class ApplicationIdService : IApplicationIdService
{
    private readonly string _applicationRegistryPath;
    private const string ApplicationIdValueName = "ApplicationId";








    public ApplicationIdService()
    {
        _applicationRegistryPath = BuildApplicationRegistryPath();
    }








    /// <summary>
    ///     Retrieves the application identifier from the application registry location, creating it when missing.
    /// </summary>
    public Guid GetApplicationId()
    {
        using RegistryKey registryKey = OpenApplicationRegistryKey();

        if (registryKey.GetValue(ApplicationIdValueName) is string rawValue && Guid.TryParse(rawValue, out Guid existingApplicationId))
        {
            return existingApplicationId;
        }

        Guid newApplicationId = Guid.NewGuid();
        registryKey.SetValue(ApplicationIdValueName, newApplicationId.ToString("D"), RegistryValueKind.String);
        return newApplicationId;
    }








    /// <summary>
    ///     Generates and persists a new application identifier in the application registry location.
    /// </summary>
    public Guid RenewApplicationId()
    {
        using RegistryKey registryKey = OpenApplicationRegistryKey();
        Guid renewedApplicationId = Guid.NewGuid();
        registryKey.SetValue(ApplicationIdValueName, renewedApplicationId.ToString("D"), RegistryValueKind.String);
        return renewedApplicationId;
    }








    private static string BuildApplicationRegistryPath()
    {
        Assembly entryAssembly = Assembly.GetEntryAssembly() ?? typeof(ApplicationIdService).Assembly;
        var productName = entryAssembly.GetName().Name ?? "RAGDataIngestionWPF";
        var companyName = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        return string.IsNullOrWhiteSpace(companyName) ? $@"Software\\{productName}" : $@"Software\\{companyName}\\{productName}";
    }








    private RegistryKey OpenApplicationRegistryKey()
    {
        return Registry.CurrentUser.CreateSubKey(_applicationRegistryPath, writable: true)
               ?? throw new InvalidOperationException($"Unable to open registry path '{_applicationRegistryPath}'.");
    }
}