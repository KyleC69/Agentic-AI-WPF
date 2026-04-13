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



namespace DataIngestionLib.Models;





public record AIModels
{


    public const string GEMMA3_4B = "gemma3:4b-cloud";

    public const string GLM5 = "glm-5:cloud";

    /// <summary>OpenAI GPT-4 cloud model identifier.</summary>
    public const string GPT4 = "gpt-4";

    /// <summary>A locally-hosted GPT-family open-source model served through Ollama.</summary>
    public const string GPTOSS120 = "gpt-oss:120b-cloud";

/// <summary>A smaller variant of the GPT-family open-source model served through Ollama, useful for testing and fallback scenarios.</summary>
public const string GPTOSS20 = "gpt-oss:20b-cloud";


    /// <summary>Meta Llama 3.1 8-billion parameter variant served through Ollama.</summary>
    public const string LLAMA1_8B = "llama3.1:8b";


    /// <summary>MixedBread AI large embedding model (<c>mxbai-embed-large</c>) served through Ollama.</summary>
    public const string MXBAI = "mxbai-embed-large:latest";


    /// <summary>All chat-capable models available for selection in the UI, ordered by preference.</summary>
    public static readonly IReadOnlyList<AIModelDescriptor> ChatModels =
    [
        new("GPT-OSS 120B", GPTOSS120),
        new("GPT-OSS 20B", GPTOSS20),
        new("Llama 3.1 8B", LLAMA1_8B),
        new("GLM-5",        GLM5),
        new("Gemma3 4B",    GEMMA3_4B),
        new("GPT-4",        GPT4),
        new("MXBAI",          MXBAI),
    ];

    /// <summary>The default model used on startup and after resets.</summary>
    public static readonly AIModelDescriptor Default = ChatModels[0];
}