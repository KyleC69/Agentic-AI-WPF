// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using SystemConfigurationManager = System.Configuration.ConfigurationManager;




namespace AgentAILib;





public class AppSettings : IAppSettings
{
    public AppSettings()
    {
        AgentId = GetAppSetting(nameof(AgentId), AgentId);
        ApplicationId = GetAppSetting(nameof(ApplicationId), ApplicationId);
        ChatHistoryConnectionString = GetAppSetting(nameof(ChatHistoryConnectionString), ChatHistoryConnectionString);
        ChatModel = GetAppSetting("ChatModelName", ChatModel);
        EmbeddingModel = GetAppSetting("EmbeddingsModelName", EmbeddingModel);
        LearnBaseUrl = GetAppSetting(nameof(LearnBaseUrl), LearnBaseUrl);
        LogDirectory = GetAppSetting(nameof(LogDirectory), LogDirectory);
        LogName = GetAppSetting(nameof(LogName), LogName);
        MaximumContext = ParseInt(GetAppSetting("MaxContextTokens", MaximumContext.ToString()), MaximumContext);
        Orchestration = ParseOrchestration(GetAppSetting("OrchestrationMode", nameof(OrchestrationMode.None)));
        RemoteRAGConnectionString = GetAppSetting(nameof(RemoteRAGConnectionString), RemoteRAGConnectionString);
        RestEndpoint = GetAppSetting(nameof(RestEndpoint), RestEndpoint);
        ResumeLast = ParseBool(GetAppSetting(nameof(ResumeLast), ResumeLast.ToString()), ResumeLast);
        UserName = GetAppSetting(nameof(UserName), UserName);
    }








    public string AgentId { get; set; } = "Agentic-Max";

    public string ApplicationId { get; set; } = "15A53D0F-041D-44DD-A150-DFB8D0F133FF";

    public string ChatHistoryConnectionString { get; set; } = "CHAT_HISTORY";

    public string ChatModel { get; set; } = "gpt-oss:cloud";

    public string EmbeddingModel { get; set; } = "mxbai-embed-large:latest";

    public string LearnBaseUrl { get; set; } = "https://learn.microsoft.com/en-us/agent-framework";

    public string LogDirectory { get; set; } = "\\logs";

    public string LogName { get; set; } = "AgenticLogs.log";

    public int MaximumContext { get; set; } = 130000;
    public OrchestrationMode Orchestration { get; set; }

    public string RemoteRAGConnectionString { get; set; } = "REMOTE_RAG";

    public string RestEndpoint { get; set; } = "http://192.168.50.4:11434";

    public bool ResumeLast { get; set; } = true;

    public string UserName { get; set; } = "TommyCat";








    private static string GetAppSetting(string key, string fallback)
    {
        var value = SystemConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }








    private static bool ParseBool(string value, bool fallback)
    {
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }








    private static int ParseInt(string value, int fallback)
    {
        return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
    }








    private static OrchestrationMode ParseOrchestration(string value)
    {
        return Enum.TryParse(value, true, out OrchestrationMode mode) ? mode : OrchestrationMode.None;
    }
}





//Agentic orchestration modes
public enum OrchestrationMode
{
    None, Concurrent, Sequential, RoundRobin
}