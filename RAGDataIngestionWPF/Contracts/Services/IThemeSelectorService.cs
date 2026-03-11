// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IThemeSelectorService.cs
// Author: Kyle L. Crowder
// Build Num: 105608



using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IThemeSelectorService
{

    AppTheme GetCurrentTheme();


    void InitializeTheme();


    void SetTheme(AppTheme theme);
}