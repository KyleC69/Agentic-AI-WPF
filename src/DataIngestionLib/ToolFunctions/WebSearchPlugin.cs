// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         WebSearchPlugin.cs
// Author: Kyle L. Crowder
// Build Num: 095205



using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;




namespace DataIngestionLib.ToolFunctions;





public sealed class WebSearchPlugin
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions WriteOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };








    public WebSearchPlugin(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _httpClient = client;
        _httpClient.BaseAddress = new Uri("https://api.langsearch.com/");
    }








    internal async Task<ToolResult<string>> ReRankResults(string documents, CancellationToken cancellationToken)
    {




        try
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "v1/rerank");

            request.Headers.UserAgent.ParseAdd("IT-Companion-WebSearchPlugin/1.0-AIAgentAssistant");
            var apiKey = Environment.GetEnvironmentVariable("LANGAPI_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return ToolResult<string>.Fail("Missing LANGAPI_KEY environment variable.");
            }

            request.Headers.Authorization = new("Bearer", apiKey);
            request.Headers.Accept.ParseAdd("application/json");

            var body = new { documents, model = "langsearch-reranker-v1" };


            var jsonBody = JsonSerializer.Serialize(body, WriteOptions);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return ToolResult<string>.Fail($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. {errorBody}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            string jsonNorm;
            try
            {
                jsonNorm = jsonResponse.Normalize(NormalizationForm.FormKC);
            }
            catch (ArgumentException)
            {
                return ToolResult<string>.Fail("Invalid Unicode content in response.");
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(jsonNorm);
            }
            catch (JsonException)
            {
                return ToolResult<string>.Ok(jsonResponse);
            }

            // 3. Pretty-print
            var pretty = JsonSerializer.Serialize(doc, WriteOptions);

            return ToolResult<string>.Ok(pretty);


        }
        catch (HttpRequestException ex)
        {
            return ToolResult<string>.Fail($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return ToolResult<string>.Fail($"Web search timed out or was canceled: {ex.Message}");
        }






    }








    [Description("Search the web for information about a topic and return summarized results with links.")]
    public async Task<ToolResult<string>> WebSearch(string strquery, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(strquery))
        {
            return ToolResult<string>.Fail("Query cannot be empty.");
        }

        if (maxResults <= 0)
        {
            return ToolResult<string>.Fail("maxResults must be greater than 0.");
        }



        try
        {

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "v1/web-search");

            request.Headers.UserAgent.ParseAdd("IT-Companion-WebSearchPlugin/1.0-AIAgentAssistant");
            var apiKey = Environment.GetEnvironmentVariable("LANGAPI_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return ToolResult<string>.Fail("Missing LANGAPI_KEY environment variable.");
            }

            request.Headers.Authorization = new("Bearer", apiKey);
            request.Headers.Accept.ParseAdd("application/json");

            var body = new { query = strquery, count = maxResults, freshness = "oneMonth", summary = false };


            var jsonBody = JsonSerializer.Serialize(body, WriteOptions);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return ToolResult<string>.Fail($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. {errorBody}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var rerankedresponse = await ReRankResults(jsonResponse, cancellationToken).ConfigureAwait(false);

            return rerankedresponse;

        }
        catch (HttpRequestException ex)
        {
            return ToolResult<string>.Fail($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return ToolResult<string>.Fail($"Web search timed out or was canceled: {ex.Message}");
        }
    }
}