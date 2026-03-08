// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AgentLogger.cs
//   Author: Kyle L. Crowder



using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class AgentLogger
{
    private readonly string _logFile;








    public AgentLogger(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        _logFile = Path.Combine(logDirectory, "agent.log");
    }








    public void Log(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var line = $"{DateTime.UtcNow:O} | {message}";
        File.AppendAllLines(_logFile, new[] { line });
    }
}