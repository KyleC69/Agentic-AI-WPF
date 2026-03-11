// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRuntimeContextAccessor.cs
// Author: Kyle L. Crowder
// Build Num: 105647



namespace DataIngestionLib.Contracts.Services;





public sealed record RuntimeContext(
        Guid ApplicationId,
        string? UserPrincipalName,
        string? DisplayName);





public interface IRuntimeContextAccessor
{
    RuntimeContext GetCurrent();
}