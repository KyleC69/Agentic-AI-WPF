// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         AppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 233154



using DataIngestionLib.Contracts;
using DataIngestionLib.Services.Contracts;




namespace RAGDataIngestionWPF.Contracts.Settings;





public class AppSettings : IAppSettings
{
    private const string UserNameSettingName = "UserName";

    /// <inheritdoc />
    public string OllamaHost
    {
        get { return GetString(nameof(OllamaHost)); }
        set { SetString(nameof(OllamaHost), value); }
    }

    /// <inheritdoc />
    public int OllamaPort
    {
        get { return GetInt(nameof(OllamaPort)); }
        set { SetInt(nameof(OllamaPort), value); }
    }

    /// <inheritdoc />
    public string ChatModel
    {
        get { return GetString(nameof(ChatModel)); }
        set { SetString(nameof(ChatModel), value); }
    }

    /// <inheritdoc />
    public string EmbeddingModel
    {
        get { return GetString(nameof(EmbeddingModel)); }
        set { SetString(nameof(EmbeddingModel), value); }
    }

    /// <inheritdoc />
    public string LearnBaseUrl
    {
        get { return GetString(nameof(LearnBaseUrl)); }
        set { SetString(nameof(LearnBaseUrl), value); }
    }

    /// <inheritdoc />
    public string LogDirectory
    {
        get { return GetString(nameof(LogDirectory)); }
        set { SetString(nameof(LogDirectory), value); }
    }

    /// <inheritdoc />
    public string LogName
    {
        get { return GetString(nameof(LogName)); }
        set { SetString(nameof(LogName), value); }
    }

    /// <inheritdoc />
    public string ChatHistoryConnectionString
    {
        get { return GetString(nameof(ChatHistoryConnectionString)); }
        set { SetString(nameof(ChatHistoryConnectionString), value); }
    }

    /// <inheritdoc />
    public string RemoteRAGConnectionString
    {
        get { return GetString(nameof(RemoteRAGConnectionString)); }
        set { SetString(nameof(RemoteRAGConnectionString), value); }
    }

    /// <inheritdoc />
    public int SessionBudget
    {
        get { return GetInt(nameof(SessionBudget)); }
        set { SetInt(nameof(SessionBudget), value); }
    }

    /// <inheritdoc />
    public int SystemBudget
    {
        get { return GetInt(nameof(SystemBudget)); }
        set { SetInt(nameof(SystemBudget), value); }
    }

    /// <inheritdoc />
    public int RAGBudget
    {
        get { return GetInt(nameof(RAGBudget)); }
        set { SetInt(nameof(RAGBudget), value); }
    }

    /// <inheritdoc />
    public int ToolBudget
    {
        get { return GetInt(nameof(ToolBudget)); }
        set { SetInt(nameof(ToolBudget), value); }
    }

    /// <inheritdoc />
    public int MetaBudget
    {
        get { return GetInt(nameof(MetaBudget)); }
        set { SetInt(nameof(MetaBudget), value); }
    }

    /// <inheritdoc />
    public int MaximumContext
    {
        get { return GetInt(nameof(MaximumContext)); }
        set { SetInt(nameof(MaximumContext), value); }
    }

    /// <inheritdoc />
    public string ApplicationId
    {
        get { return GetString(nameof(ApplicationId)); }
        set { SetString(nameof(ApplicationId), value); }
    }

    /// <inheritdoc />
    public string LastConversationId
    {
        get { return GetString(nameof(LastConversationId)); }
        set { SetString(nameof(LastConversationId), value); }
    }

    /// <inheritdoc />
    public string UserId
    {
        get { return GetString(UserNameSettingName); }
        set { SetString(UserNameSettingName, value); }
    }

    public bool ResumeLast
    {
        get { return (bool)Properties.Settings.Default[nameof(ResumeLast)]; }
        set { SetBool(nameof(ResumeLast), value); }
    }








    public TokenBudget GetTokenBudget()
    {
        return new TokenBudget
        {
                SessionBudget = SessionBudget,
                SystemBudget = SystemBudget,
                RAGBudget = RAGBudget,
                ToolBudget = ToolBudget,
                MetaBudget = MetaBudget,
                BudgetTotal = SessionBudget + SystemBudget + RAGBudget + ToolBudget + MetaBudget,
                MaximumContext = MaximumContext
        };
    }








    private static int GetInt(string settingName)
    {
        return (int)Properties.Settings.Default[settingName];
    }








    private static string GetString(string settingName)
    {
        return (string)Properties.Settings.Default[settingName] ?? string.Empty;
    }








    private static void SaveSetting(string settingName, object value)
    {
        Properties.Settings.Default[settingName] = value;
        Properties.Settings.Default.Save();
    }








    private static void SetBool(string settingName, bool value)
    {
        SaveSetting(settingName, value);
    }








    private static void SetInt(string settingName, int value)
    {
        SaveSetting(settingName, value);
    }








    private static void SetString(string settingName, string value)
    {
        SaveSetting(settingName, value ?? string.Empty);
    }
}