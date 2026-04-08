// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         BlankViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 212938



using CommunityToolkit.Mvvm.ComponentModel;

using DataIngestionLib.Agents;




namespace RAGDataIngestionWPF.ViewModels;




public sealed class BlankViewModel : ObservableObject
{




    internal ICollection<AgentDescriptor> Agents { get; } = new List<AgentDescriptor>();
    public void AddAgent(AgentDescriptor agent)
    {
        Agents.Add(agent);
    }
}