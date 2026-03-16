// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIModels.cs
// Author: Kyle L. Crowder
// Build Num: 051925



namespace DataIngestionLib.Models;





public record AIModels
{

    /// <summary>OpenAI GPT-4 cloud model identifier.</summary>
    public const string GPT4 = "gpt-4";

    /// <summary>A locally-hosted GPT-family open-source model served through Ollama.</summary>
    public const string GPTOSS = "gpt-oss:20b-cloud";

    /// <summary>Meta Llama 3.2 1-billion parameter variant served through Ollama.</summary>
    public const string LLAMA1_B = "llama3.2:1b";

    /// <summary>Meta Llama 3.2 3-billion parameter variant served through Ollama.</summary>
    public const string LLAMA323_B = "llama3.2:3b";

    /// <summary>MixedBread AI large embedding model (<c>mxbai-embed-large</c>) served through Ollama.</summary>
    public const string MXBAI = "mxbai-embed-large:latest";
}