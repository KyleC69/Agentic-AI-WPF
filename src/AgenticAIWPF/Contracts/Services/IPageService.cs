// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IPageService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using System.Windows.Controls;




namespace AgenticAIWPF.Contracts.Services;





public interface IPageService
{

    Page GetPage(string key);


    Type GetPageType(string key);
}