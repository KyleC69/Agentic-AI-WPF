// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//

using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RAGDataIngestionWPF.Models;

public class MessageDisplay
{
    
    public ChatMessage  Message{ get; set; }
    
    public ChatRole Role { get; set; }
    
    public DateTime Timestamp { get; set; }
    public string Text { get; set; }
    
    public bool IsUser => Role == ChatRole.User;


}
