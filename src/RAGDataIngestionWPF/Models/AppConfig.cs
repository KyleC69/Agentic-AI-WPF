// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         AppConfig.cs
// Author: Kyle L. Crowder
// Build Num: 091011



namespace RAGDataIngestionWPF.Models;





public class AppSettings
{
    public const string CONFIGURATION_SECTION_NAME = "AppConfig";

    public string AppPropertiesFileName { get; init; }

    public string ChatSessionFileName { get; init; }

    public string ConfigurationsFolder { get; init; }

    public string PrivacyStatement { get; init; }
    public string UserFileName { get; init; }
}