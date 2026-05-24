// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IRuntimeAppSettingsService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Contracts.Services;





public interface IRuntimeAppSettingsService
{
    string GetValue(string key, string fallback);


    void SetValue(string key, string value);
}