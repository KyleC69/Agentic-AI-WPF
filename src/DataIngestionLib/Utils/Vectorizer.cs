// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         Vectorizer.cs
// Author: Kyle L. Crowder
// Build Num: 051945



using DataIngestionLib.Agents;

using Microsoft.Agents.Core.Serialization;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Utils;





// mini utility class to vectorize text data for use in ML models, etc.
internal class Vectorizer
{

    public static async Task<string> ToVector(string text)
    {
        var generator = AgentFactory.GetEmbeddingClient();

        var embedding = await generator.GenerateAsync(text).ConfigureAwait(false);



        return embedding.Vector.ToJsonElements().ToString();
    }
}