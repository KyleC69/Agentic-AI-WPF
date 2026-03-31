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



using DataIngestionLib.Agents;

using Microsoft.Agents.Core.Serialization;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Utils;





// mini utility class to vectorize text data for use in ML models, etc.
internal class Vectorizer
{

    public static async Task<string> ToVector(string text)
    {
        LoggingEmbeddingGenerator<string, Embedding<float>> generator = AgentFactory.GetEmbeddingClient();

        Embedding<float> embedding = await generator.GenerateAsync(text).ConfigureAwait(false);



        return embedding.Vector.ToJsonElements().ToString()!;
    }
}