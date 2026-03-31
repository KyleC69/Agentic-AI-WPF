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



using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IHistoryIdentityService
{
    string GetAgentId();


    HistoryIdentity Current { get; }


    void Initialize(string applicationId, string agentId, string userId);


    void ApplyToSession(AgentSession session);
}