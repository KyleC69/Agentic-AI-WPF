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

        string result = stream.ToBase64String();

        Assert.AreEqual(Convert.ToBase64String(bytes), result);
    }
}
