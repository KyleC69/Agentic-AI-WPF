using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using DataIngestionLib.ToolFunctions;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class WebSearchPluginTests
{
    private sealed class StubHttpMessageHandler(HttpStatusCode code, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(code)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            response.Headers.Add("x-test", "true");
            return Task.FromResult(response);
        }
    }

    [TestMethod]
    public void ConstructorWithNullClientThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new WebSearchPlugin(null!));
    }

    [TestMethod]
    public async Task WebSearchWithEmptyQueryReturnsFailure()
    {
        using HttpClient client = new(new StubHttpMessageHandler(HttpStatusCode.OK, "{}"));
        WebSearchPlugin plugin = new(client);

        ToolResult<string> result = await plugin.WebSearch("  ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Query cannot be empty.", result.Error);
    }

    [TestMethod]
    public async Task WebSearchWithInvalidMaxResultsReturnsFailure()
    {
        using HttpClient client = new(new StubHttpMessageHandler(HttpStatusCode.OK, "{}"));
        WebSearchPlugin plugin = new(client);

        ToolResult<string> result = await plugin.WebSearch("query", 0);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("maxResults must be greater than 0.", result.Error);
    }

    [TestMethod]
    public async Task WebSearchWithoutApiKeyReturnsFailure()
    {
        string prior = Environment.GetEnvironmentVariable("LANGAPI_KEY");
        Environment.SetEnvironmentVariable("LANGAPI_KEY", null);

        try
        {
            using HttpClient client = new(new StubHttpMessageHandler(HttpStatusCode.OK, "{}"));
            WebSearchPlugin plugin = new(client);

            ToolResult<string> result = await plugin.WebSearch("query", 3);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Missing LANGAPI_KEY environment variable.", result.Error);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LANGAPI_KEY", prior);
        }
    }

    [TestMethod]
    public async Task WebSearchHttpErrorReturnsFailureDetails()
    {
        string prior = Environment.GetEnvironmentVariable("LANGAPI_KEY");
        Environment.SetEnvironmentVariable("LANGAPI_KEY", "test-key");

        try
        {
            using HttpClient client = new(new StubHttpMessageHandler(HttpStatusCode.BadRequest, "bad body"));
            WebSearchPlugin plugin = new(client);

            ToolResult<string> result = await plugin.WebSearch("query", 1);

            Assert.IsFalse(result.Success);
            StringAssert.StartsWith(result.Error, "HTTP 400 Bad Request.");
            StringAssert.Contains(result.Error, "bad body");
        }
        finally
        {
            Environment.SetEnvironmentVariable("LANGAPI_KEY", prior);
        }
    }
}
