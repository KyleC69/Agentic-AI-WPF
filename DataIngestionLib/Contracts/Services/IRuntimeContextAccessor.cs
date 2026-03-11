// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IRuntimeContextAccessor.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Contracts.Services;





public sealed record RuntimeContext(
        Guid ApplicationId,
        string? UserPrincipalName,
        string? DisplayName);





public interface IRuntimeContextAccessor
{
    RuntimeContext GetCurrent();
}