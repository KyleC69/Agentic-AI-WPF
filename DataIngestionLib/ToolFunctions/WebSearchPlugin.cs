// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         WebSearchPlugin.cs
//   Author: Kyle L. Crowder



using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;




namespace DataIngestionLib.ToolFunctions;





public sealed class WebSearchPlugin : IAsyncDisposable
{
    private readonly string _endPointUrl = "https://api.langsearch.com/v1/web-search";

    private readonly HttpClient _httpClient;








    public WebSearchPlugin()
    {
        _httpClient = new();
    }








    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }








    [Description("Search the web for information about a topic and return summarized results with links.")]
    public async Task<string> WebSearch(string strquery, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(strquery))
        {
            return "{Error: Query cannot be empty}";
        }

        if (maxResults <= 0)
        {
            return JsonSerializer.Serialize("Error: maxResults must be greater than 0.");
        }



        try
        {

            using HttpRequestMessage request = new(HttpMethod.Post, _endPointUrl);

            request.Headers.UserAgent.ParseAdd("IT-Companion-WebSearchPlugin/1.0-AIAgentAssistant");
            var apiKey = Environment.GetEnvironmentVariable("LANGAPI_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return JsonSerializer.Serialize("Error: Missing LANGAPI_KEY environment variable.");
            }

            request.Headers.Authorization = new("Bearer", apiKey);

            var body = new
            {
                    query = strquery,
                    count = maxResults,
                    freshness = "oneMonth",
                    summary = false
            };


            JsonSerializerOptions options = new()
            {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
            };

            var jsonBody = JsonSerializer.Serialize(body, options);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Serialize($"Error: HTTP {(int)response.StatusCode} {response.ReasonPhrase}. {errorBody}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            string jsonNorm;
            try
            {
                jsonNorm = jsonResponse.Normalize(NormalizationForm.FormKC);
            }
            catch (ArgumentException)
            {
                return JsonSerializer.Serialize("Error: Invalid Unicode content in response.");
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(jsonNorm);
            }
            catch (JsonException)
            {
                return jsonResponse;
            }

            // 3. Pretty-print
            var pretty = JsonSerializer.Serialize(doc, new JsonSerializerOptions
            {
                    WriteIndented = true
            });

            return pretty;


        }
        catch (Exception)
        {
            return JsonSerializer.Serialize("Unexpected error while performing web search.");
        }
    }
}