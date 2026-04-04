// Build Date: 2026/04/04
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationBase.cs
// Author: GitHub Copilot


namespace DataIngestionLib.Services;



public abstract class ChatConversationBase
{
    protected const string DefaultAgentId = "Agentic-Max";





    protected AppSettings Settings { get; } = new AppSettings();
}
