// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ApplicationInfoService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using System.Diagnostics;
using System.Reflection;

using AgenticAIWPF.Contracts.Services;




namespace AgenticAIWPF.Services;





public sealed class ApplicationInfoService : IApplicationInfoService
{

    public Version GetVersion()
    {
        // Set the app version in AgenticAIWPF > Properties > Package > PackageVersion
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
        return Version.TryParse(version, out Version parsedVersion) ? parsedVersion : Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);
    }
}