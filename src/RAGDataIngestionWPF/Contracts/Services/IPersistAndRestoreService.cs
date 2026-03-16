// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IPersistAndRestoreService.cs
// Author: Kyle L. Crowder
// Build Num: 051856



namespace RAGDataIngestionWPF.Contracts.Services;





public interface IPersistAndRestoreService
{

    void PersistData();


    void RestoreData();
}