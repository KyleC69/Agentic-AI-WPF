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

    public const string BGE_RERANKER = "bbjson/bge-reranker-base:latest";

    public const string GEMMA3 = "gemma3:4b-cloud";

    /// <summary>OpenAI GPT-4 cloud model identifier.</summary>
    public const string GPT4 = "gpt-4";

    /// <summary>A locally-hosted GPT-family open-source model served through Ollama.</summary>
    public const string GPTOSS = "gpt-oss:120b-cloud";

    /// <summary>Meta Llama 3.2 1-billion parameter variant served through Ollama.</summary>
    public const string LLAMA1_B = "llama3.2:1b";

    /// <summary>Meta Llama 3.2 3-billion parameter variant served through Ollama.</summary>
    public const string LLAMA323_B_LATEST = "llama3.2:latest";

    /// <summary>MixedBread AI large embedding model (<c>mxbai-embed-large</c>) served through Ollama.</summary>
    public const string MXBAI = "mxbai-embed-large:latest";

    public const string GLM5 = "glm-5:cloud";




}