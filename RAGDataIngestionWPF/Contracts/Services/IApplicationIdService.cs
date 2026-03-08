namespace RAGDataIngestionWPF.Contracts.Services;

public interface IApplicationIdService
{
    Guid GetApplicationId();

    Guid RenewApplicationId();
}
