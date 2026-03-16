// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IPageService.cs
// Author: Kyle L. Crowder
// Build Num: 051856



using System.Windows.Controls;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IPageService
{

    Page GetPage(string key);


    Type GetPageType(string key);
}