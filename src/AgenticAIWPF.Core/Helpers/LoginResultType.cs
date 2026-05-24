// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         LoginResultType.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Core.Helpers;





public enum LoginResultType
{
    Success, Unauthorized, CancelledByUser, NoNetworkAvailable, UnknownError
}