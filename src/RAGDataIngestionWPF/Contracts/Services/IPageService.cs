// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IPageService.cs
// Author: Kyle L. Crowder
// Build Num: 212930



using System.Windows.Controls;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IPageService
{

    Page GetPage(string key);


    Type GetPageType(string key);
}