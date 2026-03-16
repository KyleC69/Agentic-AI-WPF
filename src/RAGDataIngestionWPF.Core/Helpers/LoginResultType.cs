// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         LoginResultType.cs
// Author: Kyle L. Crowder
// Build Num: 051916



namespace RAGDataIngestionWPF.Core.Helpers;





public enum LoginResultType
{
    Success,
    Unauthorized,
    CancelledByUser,
    NoNetworkAvailable,
    UnknownError
}