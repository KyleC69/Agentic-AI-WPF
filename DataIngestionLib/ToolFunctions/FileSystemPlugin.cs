// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         FileSystemPlugin.cs
//   Author: Kyle L. Crowder



using System.ComponentModel;



// ReSharper disable UnusedMember.Global // Invoked via reflection by the agent framework


namespace DataIngestionLib.ToolFunctions;





public class FileSystemSearch
{

    [Description("Search the file system for a specific text pattern. Tool is similar to unix \"ls\" command.")]
    public string WriteText(
            [Description("File path")] string path,
            [Description("Text content to write")] string content)
    {
        System.IO.File.WriteAllText(path, content);
        return $"Wrote File {path}";
    }
}