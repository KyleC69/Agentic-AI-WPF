// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         SystemService.cs
// Author: Kyle L. Crowder
// Build Num: 194536



using System.Diagnostics;

using AgenticAIWPF.Contracts.Services;




namespace AgenticAIWPF.Services;





public sealed class SystemService : ISystemService
{

    public void OpenInWebBrowser(string url)
    {
        // For more info see https://github.com/dotnet/corefx/issues/10361
        ProcessStartInfo psi = new() { FileName = url, UseShellExecute = true };
        _ = Process.Start(psi);
    }
}