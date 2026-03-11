using System.ComponentModel;

using OllamaSharp;




namespace DataIngestionLib.ToolFunctions;

[OllamaTool]
[Description("Reads files from the file system. The input is a path to a file, and the output is the contents of the file.")]
public class FileSystemReaderTool(string sandBoxPage)
{
}
