// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IPersistAndRestoreService.cs
// Author: Kyle L. Crowder
// Build Num: 212930



namespace AgenticAIWPF.Contracts.Services;





public interface IPersistAndRestoreService
{

    void PersistData();


    void RestoreData();
}