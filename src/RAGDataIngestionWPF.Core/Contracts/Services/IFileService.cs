// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF.Core
// File:         IFileService.cs
// Author: Kyle L. Crowder
// Build Num: 051946



namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IFileService
{

    void Delete(string folderPath, string fileName);


    T Read<T>(string folderPath, string fileName);


    void Save<T>(string folderPath, string fileName, T content);
}