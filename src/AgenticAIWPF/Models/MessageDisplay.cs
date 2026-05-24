// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         MessageDisplay.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using Microsoft.Extensions.AI;




namespace AgenticAIWPF.Models;





public class MessageDisplay
{

    public bool IsUser
    {
        get => Role == ChatRole.User;
    }

    public ChatMessage Message { get; set; }

    public ChatRole Role { get; set; }
    public string Text { get; set; }

    public DateTime Timestamp { get; set; }
}