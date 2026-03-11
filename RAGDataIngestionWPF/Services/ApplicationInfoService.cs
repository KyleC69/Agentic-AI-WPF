// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         ApplicationInfoService.cs
//   Author: Kyle L. Crowder



using System.Diagnostics;
using System.Reflection;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public class ApplicationInfoService : IApplicationInfoService
{

    public Version GetVersion()
    {
        // Set the app version in RAGDataIngestionWPF > Properties > Package > PackageVersion
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
        return new Version(version);
    }
}