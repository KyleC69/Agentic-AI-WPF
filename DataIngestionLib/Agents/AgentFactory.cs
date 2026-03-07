using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services;
using DataIngestionLib.ToolFunctions;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;
using OllamaSharp.Models;




namespace DataIngestionLib.Agents;



//This class is intended to be an Agent Factory that will create and configure agents.
public class AgentFactory : IAgentFactory
{
    private IChatClient _innerclient;
    private ILoggerFactory _factory;
    private AIAgent _agent;
    private string modelInstructions = """
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

    private readonly ISqlVectorStore _sqlVectorStore;








    public AgentFactory(IChatClient innerclient, ILoggerFactory factory, ISqlVectorStore sqlVectorStore)
    {
        _innerclient = innerclient;
        _factory = factory;
        _sqlVectorStore = sqlVectorStore;
    }

    // returns a preconfigured ChatClientAgent with Ollama as the underlying inference mechanism, configured for coding assistance.
    public AIAgent GetCodingAssistantAgent()
    {
        //Set clients context to the model's max and let the Agent's ChatHistory Providers handle the sliding window logic.
        //This ensures that tools have maximum context available when invoked and that the session state is always normalized and persisted on disk.
        ChatOptions clioptions = new ChatOptions().AddOllamaOption(OllamaOption.NumCtx, 130000).AddOllamaOption(OllamaOption.NumThread, 4);

        IChatClient outer = new ChatClientBuilder(_innerclient).UseLogging(_factory)
                .UseFunctionInvocation()
                .ConfigureOptions(chatOptions =>
                {
                    chatOptions.Tools = ToolBuilder.GetAiTools();

                    chatOptions.Temperature = 0.7f;
                })
                .Build();



        AIMemoryProvider chatHistoryMemoryProvider = new(_sqlVectorStore);

        outer = outer.AsBuilder()
                .UseAIContextProviders(chatHistoryMemoryProvider)
                .Build();

        _agent = outer.AsAIAgent(modelInstructions, "Max", "Max is helpful AI Agent", ToolBuilder.GetAiTools(), _factory);





        // Configure the agent as needed (e.g., set up memory providers, tools, etc.)

        return _agent;
    }










}
