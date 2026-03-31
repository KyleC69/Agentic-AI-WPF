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



using CommunityToolkit.Diagnostics;

using DataIngestionLib.Agents;
using DataIngestionLib.Contracts;
using DataIngestionLib.Models;

using Microsoft.Agents.Core.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace DataIngestionLib.DocIngestion;





public sealed class ChunkMetadataGenerator
{

    private readonly IChatClient _chatClient;
    private readonly LoggingEmbeddingGenerator<string, Embedding<float>> _embedding;
    private readonly ILogger<ChunkMetadataGenerator> _logger;
    private const int KeywordMaxOutputTokens = 128;
    private const int MaximumKeywordsLength = 500;
    private const int MaximumSummaryLength = 4000;
    private const int SummaryMaxOutputTokens = 384;

    private static readonly ChatOptions KeywordOptions = new() { Temperature = 0, MaxOutputTokens = KeywordMaxOutputTokens };

    private static readonly ChatOptions SummaryOptions = new() { Temperature = 0, MaxOutputTokens = SummaryMaxOutputTokens };








    public ChunkMetadataGenerator(ILoggerFactory loggerFactory, IChatClient client)
    {
        Guard.IsNotNull(loggerFactory);
        Guard.IsNotNull(client);

        _logger = loggerFactory.CreateLogger<ChunkMetadataGenerator>();

        Uri ollamaUri = new("http://localhost:11434");
        _chatClient = new OllamaApiClient(ollamaUri, AIModels.GEMMA3);
        _embedding = AgentFactory.GetEmbeddingClient();
    }








    public async Task<GeneratedChunkMetadata> GenerateAsync(string chunkContent, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNullOrWhiteSpace(chunkContent);
        var keywords = NormalizeKeywords(await this.GenerateKeywordsAsync(chunkContent, cancellationToken).ConfigureAwait(false));
        var summary = NormalizeSummary(await this.GenerateSummaryAsync(chunkContent, cancellationToken).ConfigureAwait(false));

        return new GeneratedChunkMetadata(keywords, summary);
    }








    private async Task<string> GenerateCompletionAsync(string systemPrompt, string chunkContent, ChatOptions options, CancellationToken cancellationToken)
    {
        ChatMessage[] messages =
        [
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, chunkContent)
        ];
        try
        {
            ChatResponse response = await _chatClient.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            var text = response.Text?.Trim();

            return string.IsNullOrWhiteSpace(text) ? throw new InvalidOperationException("Metadata generation returned an empty response.") : text;
        }
        catch (InvalidOperationException aiex)
        {
            _logger.LogError(aiex, "AI service error during metadata generation: {Message}", aiex.Message);
        }
        catch (TaskCanceledException)
        {

            _logger.LogWarning("Response from LLM timed out.");
        }

        return string.Empty;
    }








    public async Task<string> GenerateEmbeddingsAsync(string chunkContent, CancellationToken cancellationToken = default)
    {

        ArgumentException.ThrowIfNullOrWhiteSpace(chunkContent);
        ReadOnlyMemory<float> vector = await _embedding.GenerateVectorAsync(chunkContent);
        return vector.ToJsonElements().ToString()!;


    }








    public async Task<string> GenerateKeywordsAsync(string chunkContent, CancellationToken cancellationToken)
    {
        const string systemPrompt = """
                                    You are a keyword extraction engine. Extract the 5-10 most important keywords from the provided text.

                                    Requirements:
                                    - Output format: comma-separated list, all lowercase.
                                    - No phrases longer than 3 words.
                                    - No filler words (the, and, of, etc.).
                                    - Prioritize nouns and technical terms.
                                    - Do NOT invent terms not present in the text.
                                    - Do NOT include duplicates.

                                    Return only the comma-separated keywords. No preamble, no labels.
                                    """;

        return await this.GenerateCompletionAsync(systemPrompt, chunkContent, KeywordOptions, cancellationToken).ConfigureAwait(false);
    }








    public async Task<string> GenerateSummaryAsync(string chunkContent, CancellationToken cancellationToken)
    {
        const string systemPrompt = """
                                    You are a summarization engine. Your task is to read the provided text and produce a concise, factual summary.

                                    Requirements:
                                    - Length: 2-4 sentences.
                                    - Style: neutral, objective, and free of opinions.
                                    - Content: capture the main ideas, purpose, and important details.
                                    - Do NOT add information that is not present in the text.
                                    - Do NOT include examples unless they appear in the text.
                                    - Do NOT reference the instructions or the task itself.

                                    Return only the summary text. No preamble, no labels.
                                    """;

        return await this.GenerateCompletionAsync(systemPrompt, chunkContent, SummaryOptions, cancellationToken).ConfigureAwait(false);
    }








    internal static string NormalizeKeywords(string keywords)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keywords);

        var normalized = string.Join(", ", keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(keyword => keyword.Trim().ToLowerInvariant()).Where(keyword => !string.IsNullOrWhiteSpace(keyword)).Distinct(StringComparer.Ordinal));

        return string.IsNullOrWhiteSpace(normalized) ? throw new InvalidOperationException("Metadata generation returned no usable keywords.") : normalized.Length <= MaximumKeywordsLength ? normalized : normalized[..MaximumKeywordsLength].TrimEnd(',', ' ');
    }








    internal static string NormalizeSummary(string summary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        var normalized = summary.Trim();

        return normalized.Length <= MaximumSummaryLength ? normalized : normalized[..MaximumSummaryLength].TrimEnd();
    }
}