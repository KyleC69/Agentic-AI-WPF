namespace DataIngestionLib.Contracts.Services;

public sealed record RuntimeContext(
        Guid ApplicationId,
        string? UserPrincipalName,
        string? DisplayName);

public interface IRuntimeContextAccessor
{
    RuntimeContext GetCurrent();
}