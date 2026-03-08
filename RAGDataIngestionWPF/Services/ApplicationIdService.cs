using System.Reflection;

using Microsoft.Win32;

using RAGDataIngestionWPF.Contracts.Services;

namespace RAGDataIngestionWPF.Services;

public sealed class ApplicationIdService : IApplicationIdService
{
    private const string ApplicationIdValueName = "ApplicationId";
    private readonly string _applicationRegistryPath;

    public ApplicationIdService()
    {
        _applicationRegistryPath = BuildApplicationRegistryPath();
    }

    /// <summary>
    /// Retrieves the application identifier from the application registry location, creating it when missing.
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
    /// Generates and persists a new application identifier in the application registry location.
    /// </summary>
    public Guid RenewApplicationId()
    {
        using RegistryKey registryKey = OpenApplicationRegistryKey();
        Guid renewedApplicationId = Guid.NewGuid();
        registryKey.SetValue(ApplicationIdValueName, renewedApplicationId.ToString("D"), RegistryValueKind.String);
        return renewedApplicationId;
    }

    private RegistryKey OpenApplicationRegistryKey()
    {
        return Registry.CurrentUser.CreateSubKey(_applicationRegistryPath, writable: true)
            ?? throw new InvalidOperationException($"Unable to open registry path '{_applicationRegistryPath}'.");
    }

    private static string BuildApplicationRegistryPath()
    {
        Assembly entryAssembly = Assembly.GetEntryAssembly() ?? typeof(ApplicationIdService).Assembly;
        string productName = entryAssembly.GetName().Name ?? "RAGDataIngestionWPF";
        string companyName = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        if (string.IsNullOrWhiteSpace(companyName))
        {
            return $@"Software\\{productName}";
        }

        return $@"Software\\{companyName}\\{productName}";
    }
}
