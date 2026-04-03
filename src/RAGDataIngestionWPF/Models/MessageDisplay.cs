// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MessageDisplay.cs
// Author: Kyle L. Crowder
// Build Num: 095213



using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Models;





public class MessageDisplay
{

    public bool IsUser
    {
        get { return Role == ChatRole.User; }
    }

    public ChatMessage Message { get; set; }

    public ChatRole Role { get; set; }
    public string Text { get; set; }

    public DateTime Timestamp { get; set; }
}