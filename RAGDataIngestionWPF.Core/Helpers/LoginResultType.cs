// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Core
//  File:         LoginResultType.cs
//   Author: Kyle L. Crowder



namespace RAGDataIngestionWPF.Core.Helpers;





public enum LoginResultType
{
    Success,
    Unauthorized,
    CancelledByUser,
    NoNetworkAvailable,
    UnknownError
}