// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ToolResultTests.cs
// Author: Kyle L. Crowder
// Build Num: 091007



using DataIngestionLib.ToolFunctions;




namespace RAGDataIngestionWPF.Tests.MSTest;





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
        Assert.AreEqual(default, result.Value);
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void FailWithNullOrWhitespaceMessageThrowsArgumentException(string message)
    {
        // Arrange / Act / Assert
        Assert.ThrowsExactly<ArgumentException>(() => ToolResult<string>.Fail(message!));
    }








    [TestMethod]
    public void FailWithValidMessageSetsSuccessFalseAndError()
    {
        // Arrange / Act
        var result = ToolResult<string>.Fail("something went wrong");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("something went wrong", result.Error);
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
    public void OkWithNullValueThrowsArgumentNullException()
    {
        // Arrange / Act / Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => ToolResult<string>.Ok(null!));
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
    }
}