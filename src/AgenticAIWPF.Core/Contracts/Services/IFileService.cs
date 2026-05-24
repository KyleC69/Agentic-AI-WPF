// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         IFileService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Core.Contracts.Services;





public interface IFileService
{

    void Delete(string folderPath, string fileName);


    T Read<T>(string folderPath, string fileName);


    void Save<T>(string folderPath, string fileName, T content);
}