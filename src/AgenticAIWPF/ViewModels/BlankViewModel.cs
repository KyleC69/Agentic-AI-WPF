// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         BlankViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 194538



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