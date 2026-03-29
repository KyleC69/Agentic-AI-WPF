// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         ApplicationInfoService.cs
// Author: Kyle L. Crowder
// Build Num: 051954



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