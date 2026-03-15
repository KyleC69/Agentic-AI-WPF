// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         AppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 091009



using DataIngestionLib.Contracts;
using DataIngestionLib.Services.Contracts;




namespace RAGDataIngestionWPF.Contracts.Settings;





public class AppSettings : IAppSettings
{
    /// <inheritdoc />
    public string OllamaHost
    {
        get { return Properties.Settings.Default.OllamaHost; }
    }





    /// <inheritdoc />
    public int OllamaPort
    {
        get { return Properties.Settings.Default.OllamaPort; }
    }





    /// <inheritdoc />
    public string ChatModel
    {
        get { return Properties.Settings.Default.ChatModel; }
    }





    /// <inheritdoc />
    public string EmbeddingModel
    {
        get { return Properties.Settings.Default.EmbeddingModel; }
    }





    /// <inheritdoc />
    public string LearnBaseUrl
    {
        get { return Properties.Settings.Default.LearnBaseUrl; }
    }





    /// <inheritdoc />
    public string LogDirectory
    {
        get { return Properties.Settings.Default.LogDirectory; }
    }





    /// <inheritdoc />
    public string ChatHistoryConnectionString
    {
        get { return Environment.GetEnvironmentVariable("CHAT_HISTORY") ?? string.Empty; }
    }





    public string RemoteRAGConnectionString
    {
        get { return Environment.GetEnvironmentVariable("REMOTE_RAG") ?? string.Empty; }
    }





    public int SessionBudget
    {
        get { return Properties.Settings.Default.SessionBudget; }
    }





    public int SystemBudget
    {
        get { return Properties.Settings.Default.SystemBudget; }
    }





    public int RAGBudget
    {
        get { return Properties.Settings.Default.RAGBudget; }
    }





    public int ToolBudget
    {
        get { return Properties.Settings.Default.ToolBudget; }
    }





    public int MetaBudget
    {
        get { return Properties.Settings.Default.MetaBudget; }
    }





    public int MaximumContext
    {
        get { return Properties.Settings.Default.MaximumContext; }
    }





    /// <inheritdoc />
    public string ApplicationId { get; set; }








    public TokenBudget GetTokenBudget()
    {
        return new TokenBudget
        {
                SessionBudget = Properties.Settings.Default.SessionBudget,
                SystemBudget = Properties.Settings.Default.SystemBudget,
                RAGBudget = Properties.Settings.Default.RAGBudget,
                ToolBudget = Properties.Settings.Default.ToolBudget,
                MetaBudget = Properties.Settings.Default.MetaBudget,
                BudgetTotal = SessionBudget + SystemBudget + RAGBudget + ToolBudget + MetaBudget,
                MaximumContext = Properties.Settings.Default.MaximumContext
        };
    }
}