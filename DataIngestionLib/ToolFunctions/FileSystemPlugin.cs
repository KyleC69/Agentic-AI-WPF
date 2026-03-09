// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         FileSystemPlugin.cs
//   Author: Kyle L. Crowder



using System.ComponentModel;



// ReSharper disable UnusedMember.Global // Invoked via reflection by the agent framework


namespace DataIngestionLib.ToolFunctions;





public class FileSystemPlugin
{

    [Description("Write text content to a file at the specified path. Creates the file if it does not exist, or overwrites it if it does.")]
    public string WriteText(
            [Description("File path")] string path,
            [Description("Text content to write")] string content)
    {
        System.IO.File.WriteAllText(path, content);
        return $"Wrote File {path}";
    }
}