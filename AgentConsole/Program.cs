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