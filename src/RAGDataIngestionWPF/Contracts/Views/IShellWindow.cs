// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IShellWindow.cs
// Author: Kyle L. Crowder
// Build Num: 095211



using System.Windows.Controls;




namespace RAGDataIngestionWPF.Contracts.Views;





public interface IShellWindow
{

    void CloseWindow();


    Frame GetNavigationFrame();


    void ShowWindow();
}