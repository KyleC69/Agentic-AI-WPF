// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IPageService.cs
// Author: Kyle L. Crowder
// Build Num: 194528



using System.Windows.Controls;




namespace AgenticAIWPF.Contracts.Services;





public interface IPageService
{

    Page GetPage(string key);


    Type GetPageType(string key);
}