// Solution: AgenticAIWPF
// Project:   AgentConsole
// File:         Program.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgentConsole;





internal class Program
{
    private static async Task Main()
    {
        Console.Title = "Agent Trace Terminal";

        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            Console.WriteLine(line);
        }
    }
}