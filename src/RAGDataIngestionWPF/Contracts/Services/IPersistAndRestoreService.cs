// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IPersistAndRestoreService.cs
// Author: Kyle L. Crowder
// Build Num: 233154



namespace RAGDataIngestionWPF.Contracts.Services;





public interface IPersistAndRestoreService
{

    void PersistData();


    void RestoreData();
}