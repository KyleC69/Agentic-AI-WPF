// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         RuntimeContextAccessor.cs
// Author: Kyle L. Crowder
// Build Num: 105616



using DataIngestionLib.Contracts.Services;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Services;





public sealed class RuntimeContextAccessor : IRuntimeContextAccessor
{
    private readonly IApplicationIdService _applicationIdService;
    private readonly IUserDataService _userDataService;








    public RuntimeContextAccessor(
            IApplicationIdService applicationIdService,
            IUserDataService userDataService)
    {
        _applicationIdService = applicationIdService;
        _userDataService = userDataService;
    }








    public RuntimeContext GetCurrent()
    {
        UserViewModel user = _userDataService.GetUser();

        return new RuntimeContext(
                _applicationIdService.GetApplicationId(),
                user?.UserPrincipalName,
                user?.Name);
    }
}