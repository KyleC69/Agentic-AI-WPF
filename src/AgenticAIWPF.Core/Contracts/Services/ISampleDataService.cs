// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         ISampleDataService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using AgenticAIWPF.Core.Models;




namespace AgenticAIWPF.Core.Contracts.Services;





public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetGridDataAsync();


    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}