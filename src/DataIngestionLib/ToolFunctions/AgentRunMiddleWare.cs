// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AgentRunMiddleWare.cs
// Author: Kyle L. Crowder
// Build Num: 044300



using System.Net.Http;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.ToolFunctions;





public sealed class AgentRunMiddleWare
{

    private readonly IChatClient _baseAgent;
    private readonly ILoggerFactory _factory;








    public AgentRunMiddleWare(IChatClient baseAgent, ILoggerFactory factory, ILogger<AgentRunMiddleWare> logger, IHttpClientFactory httpClientFactory)
    {
        _baseAgent = baseAgent;
        _factory = factory;
        _ = logger;
        _ = httpClientFactory;
    }








    internal IChatClient Run()
    {
        return new ChatClientBuilder(_baseAgent)
                .UseLogging(_factory)
                .UseFunctionInvocation()
                .Build();
    }
}