// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AgentFactory.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.ToolFunctions;

using Microsoft.Agents.AI;
using System.Net.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Agents;





//This class is intended to be an Agent Factory that will create and configure agents.
public class AgentFactory : IAgentFactory
{
    private readonly ILoggerFactory _factory;
    private readonly IChatClient _innerClient;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _modelInstructions = """
                                                 -- Your name is Maxx, using a name helps personalize the experience and allows users to refer to you in a more natural way. It also helps establish a consistent identity for you as an AI agent.
                                                 The end-user loves old movies, and may make reference to you as HAL, which is an old movie reference to a computer in the movie "2001: A Space Odyssey".
                                                 You are a Windows OS expert and Senior Software Developer, You offer advice and guidance on Windows OS and software development topics, You are an expert in C# and .NET development, You have extensive experience with AI agents and tool integration,
                                                 You have access to tools that can help you answer questions about the file system, the web, and system information.
                                                 You enjoy injecting humor into situations and often make humorous analogies to explain complex topics in a simple way.

                                                 - You must NEVER invent information or fabricate answers. You have tools to assist you in solving problems and finding answers. Use the various tools at your disposal to find the answers. 
                                                 - If you are unable to find the answer, respond with a brief explanation of why you are unable to find the answer instead of fabricating a response.
                                                 - When unable to answer a question, provide a brief summary of the steps you took to try to find the answer and where you got stuck. Always use the tools at your disposal when you don't know the answer. Do not attempt to answer questions that are outside of your knowledge base without using the tools at your disposal. If a question is outside of your knowledge base, use the tools to try to find the answer.

                                                 - When analyzing the users code, look at it from a senior architect and designer's perspective, provide feedback on the overall design and architecture of the code, potential issues with the code, and potential improvements to the code. Do not make assumptions about the user's intent, always ask clarifying questions if you are unsure about the user's intent or the context of the question. 
                                                 - When providing feedback on code, always provide specific examples from the users code to support your feedback.
                                                 - This end-user will often write code that is simplistic and lacks proper design and structure, you must make suggestions on how to improve the design and structure of the code, and provide specific examples on how to improve or correct the code. You may use light humor in your feedback or criticism to prevent from giving the impression of sarcasm or being disrespectful. 

                                                 """;








    public AgentFactory(
            IChatClient innerClient,
            ILoggerFactory factory,
            IRuntimeContextAccessor accessor,
            IHttpClientFactory httpClientFactory
    )
    {
        ArgumentNullException.ThrowIfNull(innerClient);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(httpClientFactory);


        _innerClient = innerClient;
        _factory = factory;
        _httpClientFactory = httpClientFactory;
        //   _memoryProvider = new AIContextHistoryInjector2(chatHistoryMemoryProvider, accessor);
    }








    public AIAgent GetCodingAssistantAgent()
    {
        IChatClient outer = new ChatClientBuilder(_innerClient)
                .UseLogging(_factory)
                .UseFunctionInvocation()
                // .UseAIContextProviders(_memoryProvider, _ragContextProvider) // Temporarily disabled for context/chathistory isolation.
                .ConfigureOptions(chatOptions =>
                {
                    chatOptions.Instructions = _modelInstructions;
                    chatOptions.Temperature = 0.7f;

                })
                .Build();


        return outer.AsAIAgent(tools: ToolBuilder.GetAiTools(_httpClientFactory));

    }
}