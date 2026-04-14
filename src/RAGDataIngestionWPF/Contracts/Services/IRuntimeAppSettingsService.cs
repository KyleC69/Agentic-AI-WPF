namespace AgenticAIWPF.Contracts.Services;

public interface IRuntimeAppSettingsService
{
    string GetValue(string key, string fallback);

    void SetValue(string key, string value);
}
