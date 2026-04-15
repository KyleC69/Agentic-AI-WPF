// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         ToolResultTests.cs
// Author: Kyle L. Crowder
// Build Num: 213003



using AgentAILib.ToolFunctions.Utils;




namespace AgenticAIWPF.Tests.MSTest;





/// <summary>
///     Unit tests for <see cref="ToolResult{T}" /> covering factory method contracts,
///     null-safety guards, and property invariants.
/// </summary>
[TestClass]
public class ToolResultTests
{

    [TestMethod]
    public void FailValueTypeResultReturnsFailWithError()
    {
        // Arrange / Act
        var result = ToolResult<int>.Fail("integer error");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("integer error", result.Error);
        Assert.AreEqual("integer error", result.FailureReason);
        Assert.AreEqual(default, result.Value);
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void FailWithNullOrWhitespaceMessageReturnsDefaultFailureMessage(string message)
    {
        // Arrange / Act
        var result = ToolResult<string>.Fail(message!);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("The tool operation failed.", result.Error);
        Assert.AreEqual("The tool operation failed.", result.FailureReason);
    }








    [TestMethod]
    public void FailWithValidMessageSetsSuccessFalseAndError()
    {
        // Arrange / Act
        var result = ToolResult<string>.Fail("something went wrong");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("something went wrong", result.Error);
        Assert.AreEqual("something went wrong", result.FailureReason);
        Assert.IsNull(result.Value);
    }








    [TestMethod]
    public void OkComplexObjectResultPreservesReference()
    {
        // Arrange
        List<string> list = ["a", "b", "c"];

        // Act
        var result = ToolResult<List<string>>.Ok(list);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreSame(list, result.Value);
    }








    [TestMethod]
    public void OkValueTypeResultSetsValueCorrectly()
    {
        // Arrange / Act
        var result = ToolResult<int>.Ok(42);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(42, result.Value);
        Assert.IsNull(result.Error);
    }








    [TestMethod]
    public void OkWithNullValueReturnsFailure()
    {
        // Arrange / Act
        var result = ToolResult<string>.Ok(null!);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("The tool operation completed without a value.", result.Error);
    }








    [TestMethod]
    public void OkWithValidValueSetsSuccessTrueAndValue()
    {
        // Arrange / Act
        var result = ToolResult<string>.Ok("hello");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello", result.Value);
        Assert.IsNull(result.Error);
        Assert.IsNull(result.FailureReason);
    }
}