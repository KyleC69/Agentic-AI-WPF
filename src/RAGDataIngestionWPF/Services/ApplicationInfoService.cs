// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ApplicationInfoService.cs
// Author: Kyle L. Crowder
// Build Num: 233158



using System.Diagnostics;
using System.Reflection;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public sealed class ApplicationInfoService : IApplicationInfoService
{

    public Version GetVersion()
    {
        // Set the app version in RAGDataIngestionWPF > Properties > Package > PackageVersion
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
        return Version.TryParse(version, out Version parsedVersion) ? parsedVersion : Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);
    }
}