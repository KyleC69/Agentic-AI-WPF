// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         WebSearchPluginTests.cs
// Author: Kyle L. Crowder
// Build Num: 213005



using System.Net;
using System.Text;

using DataIngestionLib.ToolFunctions;

using Moq;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class WebSearchPluginTests
{

    [TestMethod]
    public void ConstructorWithNullClientThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new WebSearchPlugin(null!));
    }








    private static IHttpClientFactory CreateMockFactory(HttpClient client)
    {
        Mock<IHttpClientFactory> mockFactory = new();
        mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return mockFactory.Object;
    }








    [TestMethod]
    public async Task WebSearchHttpErrorReturnsFailureDetails()
    {
        var prior = Environment.GetEnvironmentVariable("LANGAPI_KEY");
        Environment.SetEnvironmentVariable("LANGAPI_KEY", "test-key");

        try
        {
            using HttpClient client = new HttpClient(new StubHttpMessageHandler(HttpStatusCode.BadRequest, "bad body"));
            IHttpClientFactory factory = CreateMockFactory(client);
            WebSearchPlugin plugin = new WebSearchPlugin(factory);

            var result = await plugin.WebSearch("query", 1);

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith(result.Error, "HTTP 400 Bad Request.");
            StringAssert.Contains(result.Error, "bad body");
        }
        finally
        {
            Environment.SetEnvironmentVariable("LANGAPI_KEY", prior);
        }
    }








    [TestMethod]
    public async Task WebSearchWithEmptyQueryReturnsFailure()
    {
        using HttpClient client = new HttpClient(new StubHttpMessageHandler(HttpStatusCode.OK, "{}"));
        IHttpClientFactory factory = CreateMockFactory(client);
        WebSearchPlugin plugin = new WebSearchPlugin(factory);

        var result = await plugin.WebSearch("  ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Query cannot be empty.", result.Error);
    }








    [TestMethod]
    public async Task WebSearchWithInvalidMaxResultsReturnsFailure()
    {
        using HttpClient client = new HttpClient(new StubHttpMessageHandler(HttpStatusCode.OK, "{}"));
        IHttpClientFactory factory = CreateMockFactory(client);
        WebSearchPlugin plugin = new WebSearchPlugin(factory);

        var result = await plugin.WebSearch("query", 0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be greater than 0.", result.Error);
    }








    [TestMethod]
    public async Task WebSearchWithoutApiKeyReturnsFailure()
    {
        var prior = Environment.GetEnvironmentVariable("LANGAPI_KEY");
        Environment.SetEnvironmentVariable("LANGAPI_KEY", null);

        try
        {
            using HttpClient client = new HttpClient(new StubHttpMessageHandler(HttpStatusCode.OK, "{}"));
            IHttpClientFactory factory = CreateMockFactory(client);
            WebSearchPlugin plugin = new WebSearchPlugin(factory);

            var result = await plugin.WebSearch("query", 3);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Missing LANGAPI_KEY environment variable.", result.Error);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LANGAPI_KEY", prior);
        }
    }








    private sealed class StubHttpMessageHandler(HttpStatusCode code, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
            response.Headers.Add("x-test", "true");
            return Task.FromResult(response);
        }
    }
}