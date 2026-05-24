// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IPersistAndRestoreService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Contracts.Services;





public interface IPersistAndRestoreService
{

    void PersistData();


    void RestoreData();
}