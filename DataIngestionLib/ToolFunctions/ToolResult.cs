// 2026/03/09
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ToolResult.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.ToolFunctions;




//Easy to use result type for tools, encapsulates success/failure and value or error message.
public sealed class ToolResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static ToolResult<T> Ok(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new() { Success = true, Value = value };
    }

    public static ToolResult<T> Fail(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Failure message cannot be null or whitespace.", nameof(message));
        }

        return new() { Success = false, Error = message };
    }
}