// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IShellWindow.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using System.Windows.Controls;




namespace AgenticAIWPF.Contracts.Views;





public interface IShellWindow
{

    void CloseWindow();


    Frame GetNavigationFrame();


    void ShowWindow();
}