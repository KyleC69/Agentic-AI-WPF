// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         ISampleDataService.cs
// Author: Kyle L. Crowder
// Build Num: 194525



using AgenticAIWPF.Core.Models;




namespace AgenticAIWPF.Core.Contracts.Services;





public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetGridDataAsync();


    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}