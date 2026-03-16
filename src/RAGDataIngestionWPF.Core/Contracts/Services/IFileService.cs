// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         IFileService.cs
// Author: Kyle L. Crowder
// Build Num: 051916



namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IFileService
{

    void Delete(string folderPath, string fileName);


    T Read<T>(string folderPath, string fileName);


    void Save<T>(string folderPath, string fileName, T content);
}