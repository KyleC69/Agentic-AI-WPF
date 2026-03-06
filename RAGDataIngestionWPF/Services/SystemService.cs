// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         SystemService.cs
//   Author: Kyle L. Crowder



using System.Diagnostics;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public class SystemService : ISystemService
{

    public void OpenInWebBrowser(string url)
    {
        // For more info see https://github.com/dotnet/corefx/issues/10361
        ProcessStartInfo psi = new()
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}