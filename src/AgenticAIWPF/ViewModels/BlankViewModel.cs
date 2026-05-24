// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         BlankViewModel.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using AgentAILib.Agents;

using CommunityToolkit.Mvvm.ComponentModel;




namespace AgenticAIWPF.ViewModels;





public sealed class BlankViewModel : ObservableObject
{

    internal ICollection<AgentDescriptor> Agents { get; } = new List<AgentDescriptor>();








    public void AddAgent(AgentDescriptor agent)
    {
        Agents.Add(agent);
    }
}