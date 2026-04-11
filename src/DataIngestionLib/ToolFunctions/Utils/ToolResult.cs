// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ToolResult.cs
// Author: Kyle L. Crowder
// Build Num: 212923

namespace DataIngestionLib.ToolFunctions.Utils;





/// <summary>
///     Represents a result type that encapsulates the outcome of a tool operation,
///     indicating success or failure, and optionally providing a value or an error message.
/// </summary>
/// <typeparam name="T">
///     The type of the value contained in the result, if the operation is successful.
/// </typeparam>
public sealed class ToolResult<T>
{
    public string? Error { get; init; }
    public bool Success { get; init; }
    public T? Value { get; init; }
    public string? FailureReason { get; init; }







    public static ToolResult<T> Fail(string message)
    {
        var normalizedMessage = string.IsNullOrWhiteSpace(message) ? "The tool operation failed." : message.Trim();

        return new()
        {
            Success = false,
            Error = normalizedMessage,
            FailureReason = normalizedMessage
        };
    }








    public static ToolResult<T> Ok(T value)
    {
        if (value == null)
        {
            return Fail("The tool operation completed without a value.");
        }

        return new()
        {
            Success = true,
            Value = value,
            Error = null,
            FailureReason = null
        };
    }
}