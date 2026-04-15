// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IRuntimeAppSettingsService.cs
// Author: Kyle L. Crowder
// Build Num: 194528



namespace AgenticAIWPF.Contracts.Services;





public interface IRuntimeAppSettingsService
{
    string GetValue(string key, string fallback);


    void SetValue(string key, string value);
}