// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IngestQualityControl.cs
// Author: Kyle L. Crowder
// ###########
// ## This process is currently use experimental API's in the Agent Framework, which is changing rapidly. Use at your own risk acknowledging it may change significantly.
// ## MAF has recently introduced a much easier way of using structured output. You no longer have to create your own json schemas.
// ## Here I am just passing a simple enum to the LLM, but you can pass more complex data structures if needed, and the LLM will be able to understand and work with them,
// ## this is a powerful feature of the MAF that allows for more complex and nuanced interactions with the LLM, and can help to improve the quality and relevance of the output.
// ## It requires that the client implement the IChatClient interface, and that the LLM is able to understand and work with the data structures being passed to it, but it can be a powerful tool for improving the quality and relevance of the output from the LLM.







    /// <summary>
    ///     Initiates the asynchronous quality control process for a collection of data items by validating each item.
    /// </summary>
    /// <remarks>
    ///     Invalid data items are handled by logging a message for each item that fails validation.
    ///     Ensure that the data items are in a valid format before invoking this method.
    /// </remarks>
    /// <param name="dataItems">
    ///     An enumerable collection of data items to validate. Each item is processed individually to determine its
    ///     validity.
    /// </param>
    /// <returns>A task that represents the asynchronous operation of starting the quality control process.</returns>
    Task StartQualityControlAsync(IEnumerable<DocPage> dataItems);








    Task TestIngestQualityControlAsync();








    /// <inheritdoc />
    Task<bool> ValidateDataAsync(string data);
}





public sealed class IngestQualityControl
{
    private readonly IAgentFactory _agentFactory;
    private readonly IDocRepository _docRepository;




    private readonly ILoggerFactory _factory;
    private readonly List<DocPage> _finalValuableList;
    private readonly List<DocPage> _garbageList;
    private readonly List<DocPage> _unknownList;
    private readonly List<DocPage> _unsuitableForRagList;

    // Pipeline gate
    private SemaphoreSlim _evaluationSemaphore = new(1, 1);
    private ILogger<IngestQualityControl> _logger;








    /// <summary>
    ///     Initializes a new instance of <see cref="IngestQualityControl" /> with dependencies
    ///     provided by the DI container.
    /// </summary>
    /// <param name="agentFactory">Factory used to create chat-agent instances.</param>
    /// <param name="loggerFactory">Logger factory for creating typed loggers.</param>
    /// <param name="docRepository">Repository for accessing document data.</param>
    public IngestQualityControl(IAgentFactory agentFactory, ILoggerFactory loggerFactory, IDocRepository docRepository)
    {
        _agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
        _factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = _factory.CreateLogger<IngestQualityControl>();
        _garbageList = new List<DocPage>();
        _unknownList = new List<DocPage>();
        _finalValuableList = new List<DocPage>();
        _docRepository = docRepository ?? throw new ArgumentNullException(nameof(docRepository));
        _unsuitableForRagList = new();



    }








    public List<StructuredResults> RagResults { get; set; }








    private async Task ConstructTopics(DocPage doc)
    {
        // Last step in the evaluation process, this is where we take the content that has been deemed "valuable"
        // and had "RAGQuality" and organize it in a way that is suitable for prompt injection and can produce a valuable vector index for retrieval.
        // by this point the text should be well-structured and have semantic value, so we can ask the LLM to organize it into topics and content blocks that can be used for retrieval in a RAG system.

        ChatClientAgent chatClient = _agentFactory.Create(
                """

                You are a topic segmenter for RAG preparation.
                Reorganize the input into semantic segments and return a list of items with `Topic` and `Content`.

                  Output rules:
                  - `Topic`: short heading, 3–8 words, not a full sentence, not duplicated content.
                  - `Content`: multi-sentence block (≥30 chars) extracted or lightly reorganized from the input; no placeholders, headings, or invented text.
                  - Every item must include both `Topic` and `Content`; no empty strings or nulls.
                  - Preserve the author’s intent while improving clarity and structure.

                  Example:
                  [
                    { "Topic": "Introduction to the Framework", "Content": "The framework provides a comprehensive set of tools and libraries for building AI applications, including support for natural language processing, machine learning, and deep learning. It also offers pre-built models and components that can be easily integrated into your applications." },
                    { "Topic": "Key Features", "Content": "The framework includes features such as model training and deployment, data preprocessing, and model evaluation. It also provides support for distributed computing and GPU acceleration, making it suitable for large-scale AI projects." }
                  ]

                """
        );


        try
        {
            var response = await chatClient.RunAsync<List<StructuredResults>>(doc.RawPageSource).ConfigureAwait(false);

            if (response.Result != null)
            {
                await SaveNewSectionsAsync(response.Result, doc).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidModelResponse("Model returned an empty or null response for structured results.");
            }


        }
        catch (InvalidModelResponse ex)
        {
            _logger.LogError(ex, "Model returned an invalid response for document with ID {DocumentId}: {ResponseResult}", doc.Id, ex.Message);
            await FurtherEvaluation(doc).ConfigureAwait(false);
            _evaluationSemaphore.Release();
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while evaluating document with ID {DocumentId}. Marking for further evaluation.", doc.Id);
            await FurtherEvaluation(doc).ConfigureAwait(false);
            _evaluationSemaphore.Release();
        }
        finally
        {
            chatClient.ChatClient.Dispose();
            _evaluationSemaphore.Release();
        }
    }








    public async Task EvaluateDocument(DocPage doc)
    {
        //Limits one doc in testing pipeline at a time.
        await _evaluationSemaphore.WaitAsync().ConfigureAwait(false);



        ChatClientAgent chatClient = _agentFactory.Create(
                """
                    You are a documentation structure reviewer.
                    Classify the text by structural quality and semantic richness (ignore domain meaning).
                    Output exactly one of: Garbage | Valuable | Unknown.

                    Valuable: coherent paragraph or more, multiple complete sentences, provides context and detail.
                    Garbage: single sentence or fragment, vague generic statements (e.g., "This is a file."), mostly tags/markup (e.g., <file>, <code>), or keyword lists with no prose.
                    Unknown: borderline cases that are not clearly valuable or garbage.
                """
        );
        try
        {
            var response = await chatClient.RunAsync<DocumentValue>(doc.RawPageSource).ConfigureAwait(false);

            switch (response.Result)
            {

                case DocumentValue.Unknown:
                    await FurtherEvaluation(doc).ConfigureAwait(false);
                    _evaluationSemaphore.Release();
                    break;
                case DocumentValue.Garbage:
                    await HandleGarbage(doc).ConfigureAwait(false);
                    _evaluationSemaphore.Release();
                    break;
                case DocumentValue.Valuable:
                    await EvaluateRagValue(doc).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidModelResponse(response.Result.ToString());
            }
        }
        catch (InvalidModelResponse ex)
        {
            _logger.LogError(ex, "Model returned an invalid response for document with ID {DocumentId}: {ResponseResult}", doc.Id, ex.Message);
            await FurtherEvaluation(doc).ConfigureAwait(false);
            _evaluationSemaphore.Release();
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while evaluating document with ID {DocumentId}. Marking for further evaluation.", doc.Id);
            await FurtherEvaluation(doc).ConfigureAwait(false);
            _evaluationSemaphore.Release();

        }
        finally
        {
            chatClient.ChatClient.Dispose();
        }

    }








    private async Task EvaluateRagValue(DocPage doc)
    {




        ChatClientAgent chatClient = _agentFactory.Create(
                """
                You are a RAG suitability reviewer.
                Classify the text for retrieval usefulness (structure + semantic richness, ignore domain meaning).
                Output exactly one of: Suitable | NotSuitable | Unknown.

                Suitable: coherent paragraph or more, multiple complete sentences, clear context and detail that could aid retrieval.
                NotSuitable: single sentence/fragment, vague generic statements (e.g., "This is a file."), mostly tags/markup (e.g., <file>, <code>), or keyword lists with no prose.
                Unknown: borderline cases not clearly suitable or unsuitable.
                """
        );


        try
        {
            var response = await chatClient.RunAsync<RagSuitability>(doc.RawPageSource).ConfigureAwait(false);

            switch (response.Result)
            {

                case RagSuitability.Unknown:
                    await FurtherEvaluation(doc).ConfigureAwait(false);
                    _evaluationSemaphore.Release();
                    break;
                case RagSuitability.NotSuitable:
                    await HandleUnsuitable(doc).ConfigureAwait(false);
                    _evaluationSemaphore.Release();
                    break;
                case RagSuitability.Suitable:
                    await ConstructTopics(doc).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidModelResponse(response.Result.ToString());
            }
        }
        catch (InvalidModelResponse ex)
        {
            _logger.LogError(ex, "Model returned an invalid response for document with ID {DocumentId}: {ResponseResult}", doc.Id, ex.Message);
            await FurtherEvaluation(doc).ConfigureAwait(false);
            _evaluationSemaphore.Release();
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while evaluating document with ID {DocumentId}. Marking for further evaluation.", doc.Id);
            await FurtherEvaluation(doc).ConfigureAwait(false);
            _evaluationSemaphore.Release();

        }
        finally
        {
            chatClient.ChatClient.Dispose();
        }

    }








    /// <summary>
    ///     Evaluates the specified document and marks its evaluation status as unknown.
    /// </summary>
    /// <remarks>This method adds the document to an internal list of unknown evaluations.</remarks>
    /// <param name="doc">The document to be evaluated. Cannot be null.</param>
    /// <returns>This method does not return a value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="doc" /> is null.</exception>
    private async Task FurtherEvaluation(DocPage doc)
    {
        if (doc == null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        doc.Llmeval = "Unknown";
        _unknownList.Add(doc);
    }








    /// <summary>
    ///     Handles a document that has been classified as garbage.
    /// </summary>
    /// <param name="doc">The document to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the document is null.</exception>
    private async Task HandleGarbage(DocPage doc)
    {
        if (doc == null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        doc.Isgarbage = true;
        _garbageList.Add(doc);
    }








    private async Task HandleUnsuitable(DocPage doc)
    {
        doc.Llmeval = "LLM deemed content unsuitable for RAG use.";
        _unsuitableForRagList.Add(doc);
    }








    private async Task SaveNewSectionsAsync(List<StructuredResults> sections, DocPage doc)
    {
        foreach (StructuredResults section in sections)
        {
            var idx = 0;
            DocSection sec = new()
            {
                    Id = Guid.NewGuid(),
                    DocPageId = doc.Id,
                    SemanticUid = HashUtils.ComputeSemanticUidForSection(doc.SemanticUid, section.Topic, 1, idx),
                    Heading = section.Topic,
                    Level = 1, //Was used when scraping html Relevance is minimal and may be removed in future. Document reconstruction is not a factor.
                    ContentMarkdown = section.Content,
                    OrderIndex = idx, //Todo: This normally should be determined by the structure of the document, but for simplicity, we are just using the index of the section in the response list.
                    VersionNumber = 1, // This will be determined by the versioning strategy of the application, for simplicity, we are just using 0 for all sections. Upsert will take care of versioning and incrementing this number as needed.
                    CreatedIngestionRunId = doc.CreatedIngestionRunId,
                    UpdatedIngestionRunId = null,
                    RemovedIngestionRunId = null,
                    ValidFromUtc = DateTime.Now,
                    IsActive = true,
                    ContentHash = HashUtils.ComputeSha256(section.Content),
                    DocPage = null,
                    RemovedIngestionRun = null,
                    UpdatedIngestionRun = null
            };

            idx++;

            try
            {

                await _docRepository.InsertSection(sec).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving section in db");
            }
        }


    }








    /// <summary>
    ///     Initiates the asynchronous quality control process for a collection of data items by validating each item.
    /// </summary>
    /// <remarks>
    ///     Invalid data items are handled by logging a message for each item that fails validation.
    ///     Ensure that the data items are in a valid format before invoking this method.
    /// </remarks>
    /// <param name="dataItems">
    ///     An enumerable collection of data items to validate. Each item is processed individually to determine its
    ///     validity.
    /// </param>
    /// <returns>A task that represents the asynchronous operation of starting the quality control process.</returns>
    public async Task StartQualityControlAsync(IEnumerable<DocPage> dataItems)
    {
        if (dataItems.Count() == 0)
        {
            _logger.LogInformation("No data items provided for quality control.");
            return;
        }

        foreach (DocPage data in dataItems)
        {
            await EvaluateDocument(data).ConfigureAwait(false);
        }
    }








    //Run test text through the pipeline
    public async Task TestIngestQualityControlAsync()
    {

        //Garbage input example, this is the type of content that is considered "garbage" and should be discarded, as it lacks context, detail, and meaningful information about the content.
        var garbageTest = """
                          This is a file. Contains code. <file> <code> Programming, C#, .NET Error messages, troubleshooting
                          """;



        //This is an example of content that is valuable, as it has complete sentences that provide context and detail about the content,
        //and does not contain excessive tags or metadata that do not provide meaningful information about the content.
        var nonGarbage = """
                         The framework also provides foundational building blocks, 
                         including model clients (chat completions and responses), 
                         an agent session for state management, context providers for agent memory,
                          middleware for intercepting agent actions, and MCP clients for tool integration. 
                          Together, these components give you the flexibility
                           and power to build interactive, robust, and safe AI applications.
                         """;

        // This is an example of content that is neither clearly garbage nor clearly valuable, and may require further analysis or context to determine its value.
        // It is intentionally in a gray area to test the LLM's understanding of the criteria. It is a short sentence, but it is a complete sentence and provides sentiment.
        // It is important to note that the classification of content as "garbage" or "valuable" can be subjective and may depend on the specific use case or context in which the content is being evaluated.
        // This should however be caught in the RAG evaluation, as it is not clearly garbage, but also not clearly valuable in the intended usage patterns.
        var unknown = """
                      It is a bright and sunny day.
                      """;


        // Run the test inputs through the evaluation process
        await EvaluateDocument(new DocPage { RawPageSource = garbageTest }).ConfigureAwait(false);
        await EvaluateDocument(new DocPage { RawPageSource = nonGarbage }).ConfigureAwait(false);
        await EvaluateDocument(new DocPage { RawPageSource = unknown }).ConfigureAwait(false);

        Debugger.Break();
    }








    /// <inheritdoc />
    public Task<bool> ValidateDataAsync(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            _logger.LogDebug("ValidateDataAsync: rejecting empty or whitespace-only input.");
            return Task.FromResult(false);
        }

        // TODO: Replace stub logic with LLM-assisted quality scoring.
        // The stub conservatively rejects all content to avoid false positives
        // until a real validation strategy is implemented.
        _logger.LogDebug("ValidateDataAsync: stub returning false for '{DataPreview}'.", data[..Math.Min(60, data.Length)]);
        return Task.FromResult(false);
    }








    [JsonArray]
    public sealed class StructuredResults
    {
        [JsonRequired] public required string Content { get; set; }

        [JsonRequired] public required string Topic { get; set; }
    }





    public enum DocumentValue
    {
        Unknown, Garbage, Valuable
    }





    public enum RagSuitability
    {
        Unknown, NotSuitable, Suitable
    }
}





public sealed class InvalidModelResponse : Exception
{
    public InvalidModelResponse(string responseResult)
    {
        throw new Exception($"The model returned an invalid response: {responseResult}");
    }
}