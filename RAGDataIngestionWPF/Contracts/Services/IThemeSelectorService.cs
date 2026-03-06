// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         IThemeSelectorService.cs
//   Author: Kyle L. Crowder



using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IThemeSelectorService
{

    AppTheme GetCurrentTheme();


    void InitializeTheme();


    void SetTheme(AppTheme theme);
}