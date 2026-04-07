// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         StreamExtensionsTests.cs
// Author: Kyle L. Crowder
// Build Num: 213002



using System.Text;

using RAGDataIngestionWPF.Core.Helpers;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class StreamExtensionsTests
{
    [TestMethod]
    public void ToBase64StringConvertsEntireStream()
    {
        var bytes = Encoding.UTF8.GetBytes("hello base64");
        using MemoryStream stream = new(bytes);

        var result = stream.ToBase64String();

        Assert.AreEqual(Convert.ToBase64String(bytes), result);
    }
}