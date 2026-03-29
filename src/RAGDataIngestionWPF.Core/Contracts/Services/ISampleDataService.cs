// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF.Core
// File:         ISampleDataService.cs
// Author: Kyle L. Crowder
// Build Num: 051946



using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetGridDataAsync();


    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}