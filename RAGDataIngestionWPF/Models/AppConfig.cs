// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         AppConfig.cs
//   Author: Kyle L. Crowder



namespace RAGDataIngestionWPF.Models;





public class AppSettings
{
    public const string ConfigurationSectionName = "AppConfig";

    public string AppPropertiesFileName { get; set; }

    public string ChatSessionFileName { get; set; }

    public string ConfigurationsFolder { get; set; }

    public string PrivacyStatement { get; set; }
    public string UserFileName { get; set; }
}