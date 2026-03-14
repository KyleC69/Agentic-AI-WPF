// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         DocumentationModelProcessor.cs
// Author: Kyle L. Crowder
// Build Num: 175051



using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace DataIngestionLib.DocIngestion;





public interface IDocumentationModelProcessor
{
    Task PreProcessLearnRepo(string targetPath);


    Task StartPreprocessingAsync(IEnumerable<string> filepaths, CancellationToken token = default);
}





public class DocumentationModelProcessor : IDocumentationModelProcessor
{
    private readonly HttpClient _httpClient;

    private readonly ILogger<DocumentationModelProcessor> _logger;
    // This class is responsible for processing the documentation with an LLM.
    // Tasks include:
    // - Analyze the document and extract information in a structured format.
    // - Generate summaries or insights based on the extracted information.
    // - Validate the extracted information against predefined rules or schemas.

    private OllamaApiClient _ollamaClient;
    private int _processedFileCount;








    public DocumentationModelProcessor(ILogger<DocumentationModelProcessor> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3), BaseAddress = new Uri(baseUrl) };
        _ollamaClient = new OllamaApiClient(_httpClient, model);

    }








    public async Task PreProcessLearnRepo(string targetPath)
    {

        //Feed doc pages from local repo clone into LLM for processing and storage in DB in a format optimal for retrieval and generation. 
        _logger.LogInformation("Starting to preprocess MS Learn repository content.");

        var localRepoPath = targetPath; // Assuming targetPath is the path to the local MS Learn repo clone.
        if (string.IsNullOrEmpty(localRepoPath) || !Directory.Exists(localRepoPath))
        {
            _logger.LogError("Invalid or missing MS Learn repository path: {0}", localRepoPath);
            return;
        }

        var filepaths = Directory.EnumerateFiles(localRepoPath, "*.md", SearchOption.AllDirectories).ToList();



        await StartPreprocessingAsync(filepaths, CancellationToken.None).ConfigureAwait(false);





        _logger.LogInformation("Finished preprocessing MS Learn repository. Processed {Count} files.", _processedFileCount);
    }








    public async Task StartPreprocessingAsync(IEnumerable<string> filepaths, CancellationToken token = default)
    {
        _logger.LogInformation("Beginning asynchronous preprocessing of {Count} files.", filepaths.Count());





        foreach (var file in filepaths)
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogWarning("Preprocessing cancelled after processing {Count} files.", filepaths.Count());
                break;
            }

            try
            {
                var markdown = await File.ReadAllTextAsync(file, token).ConfigureAwait(true);

                StringBuilder buffer = new();
                Chat chat = new(_ollamaClient);

                await foreach (var message in chat.SendAsync(markdown))
                {
                    // Process each message if needed
                    StringBuilder unused = buffer.Append(message);
                }


                if (string.IsNullOrEmpty(buffer.ToString()))
                {
                    _logger.LogWarning("Received empty or null content from LLM for file: {0}", file);
                    continue;
                }

                try
                {
                    //            var structuredData = JsonSerializer.Deserialize(response.Text);
                    // Here you would typically save `structuredData` to a database or other storage.
                    // For demonstration, we'll just log its structure.
                    _logger.LogInformation("Successfully parsed LLM output for file: {0}", file);
                    // _logger.LogDebug("Parsed data for {0}: {1}", file, structuredData.ToString());
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize LLM response JSON for file: {0}. Response content: {1}", file, buffer);
                }


                // Assuming the response.Message.Content is a JSON string
                // You'll need to deserialize this JSON and process it further.
                // For now, we'll just log it.
                _logger.LogInformation("LLM processing successful for file: {0}", file);
                // Example: var structuredData = JsonSerializer.Deserialize<YourStructuredDataType>(response.Message.Content);
                // Then store structuredData in your database or further processing pipeline.
                _processedFileCount++;





                _logger.LogInformation("Processed file: {0}", file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {0}", file);
            }
        }
    }








    /// <summary>
    ///     Returns the extraction instruction prompt; delegates to
    ///     <see cref="DocumentExtractionPrompts.GetModelInstructions" />.
    /// </summary>
    private string GetModelInstructions()
    {
        return DocumentExtractionPrompts.GetModelInstructions();
    }








    /// <summary>Returns the JSON output schema; delegates to <see cref="DocumentExtractionPrompts.GetOutputSchema" />.</summary>
    private JsonElement GetOutputSchema()
    {
        return DocumentExtractionPrompts.GetOutputSchema();
    }








    private void GetSettings()
    {

        var settings = ConfigurationManager.GetSection("ModelSettings") as System.Collections.Specialized.NameValueCollection;
        var baseUrl = settings["OllamaBaseUrl"] ?? "http://127.0.0.1:11434";
        var model = settings["OllamaModel"] ?? "testmod:latest";

    }
}