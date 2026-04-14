// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IShellWindow.cs
// Author: Kyle L. Crowder
// Build Num: 212930



using System.Windows.Controls;




namespace AgenticAIWPF.Contracts.Views;





public interface IShellWindow
{

    void CloseWindow();


    Frame GetNavigationFrame();


    void ShowWindow();
}