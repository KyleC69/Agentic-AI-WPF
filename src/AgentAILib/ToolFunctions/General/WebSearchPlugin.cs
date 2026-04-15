// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.General;





public sealed class WebSearchPlugin
{
    private readonly HttpClient _httpClient;


    private static readonly JsonSerializerOptions WriteOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };








    public WebSearchPlugin(IHttpClientFactory client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _httpClient = client.CreateClient(nameof(WebSearchPlugin));
        _httpClient.BaseAddress = new Uri("https://api.langsearch.com/");
        _httpClient.Timeout = TimeSpan.FromMinutes(3);
    }








    private static string CleanResponseText(string? value)
    {
        return DiagnosticsText.CleanModelText(value, 24000);
    }








    private static async Task<ToolResult<string>> CreateErrorResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return ToolResult<string>.Fail($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. {CleanResponseText(errorBody)}");
    }








    private static ToolResult<string> CreateJsonResponseResult(string responseText)
    {
        var cleanedText = CleanResponseText(responseText);

        try
        {
            using JsonDocument document = JsonDocument.Parse(cleanedText);
            return ToolResult<string>.Ok(CleanResponseText(JsonSerializer.Serialize(document, WriteOptions)));
        }
        catch (JsonException)
        {
            return ToolResult<string>.Ok(cleanedText);
        }
    }








    internal async Task<ToolResult<string>> ReRankResults(string documents, CancellationToken cancellationToken)
    {




        try
        {
            using HttpRequestMessage request = new(HttpMethod.Post, "v1/rerank");

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
                return await CreateErrorResultAsync(response, cancellationToken).ConfigureAwait(false);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return CreateJsonResponseResult(jsonResponse);


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








    public static string SanitizeControlCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        StringBuilder sb = new(input.Length);

        foreach (var c in input)
        {
            if (char.IsControl(c) && c != '\t' && c != '\n' && c != '\r')
            {
                _ = sb.Append(' ');
                // Replace with space
            }
            else
            {
                _ = sb.Append(c);
            }
        }

        return sb.ToString();
    }








    [Description("Search the web for information about a topic and return summarized results with links.")]
    public async Task<ToolResult<string>> WebSearch([Description("The topic or query to search for.")] string query, [Description("Maximum number of results to request from the provider.")] int maxResults = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ToolResult<string>.Fail("Query cannot be empty.");
        }

        if (maxResults <= 0)
        {
            return ToolResult<string>.Fail("maxResults must be greater than 0.");
        }



        try
        {

            using HttpRequestMessage request = new(HttpMethod.Post, "v1/web-search");

            request.Headers.UserAgent.ParseAdd("IT-Companion-WebSearchPlugin/1.0-AIAgentAssistant");
            var apiKey = Environment.GetEnvironmentVariable("LANGAPI_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return ToolResult<string>.Fail("Missing LANGAPI_KEY environment variable.");
            }

            request.Headers.Authorization = new("Bearer", apiKey);
            request.Headers.Accept.ParseAdd("application/json");

            var body = new { query, count = maxResults, freshness = "oneMonth", summary = false };


            var jsonBody = JsonSerializer.Serialize(body, WriteOptions);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return await CreateErrorResultAsync(response, cancellationToken).ConfigureAwait(false);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return CreateJsonResponseResult(jsonResponse);

        }
        catch (HttpRequestException ex)
        {
            return ToolResult<string>.Fail($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return ToolResult<string>.Fail($"Web search timed out or was canceled: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ToolResult<string>.Fail($"Web search failed: {ex.Message}");
        }
    }
}