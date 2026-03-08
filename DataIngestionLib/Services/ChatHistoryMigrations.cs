// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatHistoryMigrations.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Services;





internal static class ChatHistoryMigrations
{
    public static IReadOnlyList<(string Id, string Sql)> All { get; } =
    [
            (
                    "2026030701_CreateMigrationLedger",
                    """
                    IF OBJECT_ID(N'dbo.__ChatHistoryMigrations', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.__ChatHistoryMigrations
                        (
                            [Id] nvarchar(64) NOT NULL,
                            [AppliedOnUtc] datetimeoffset(7) NOT NULL,
                            CONSTRAINT [PK___ChatHistoryMigrations] PRIMARY KEY CLUSTERED ([Id] ASC)
                        );
                    END;
                    """
            ),
            (
                    "2026030702_CreateChatHistoryMessages",
                    """
                    IF OBJECT_ID(N'dbo.ChatHistoryMessages', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.ChatHistoryMessages
                        (
                            [MessageId] uniqueidentifier NOT NULL,
                            [ConversationId] nvarchar(128) NOT NULL,
                            [SessionId] nvarchar(128) NOT NULL,
                            [AgentId] nvarchar(128) NOT NULL,
                            [UserId] nvarchar(128) NOT NULL,
                            [ApplicationId] nvarchar(128) NOT NULL,
                            [Role] nvarchar(32) NOT NULL,
                            [Content] nvarchar(max) NOT NULL,
                            [TimestampUtc] datetimeoffset(7) NOT NULL,
                            [Metadata] nvarchar(max) NULL,
                            CONSTRAINT [PK_ChatHistoryMessages] PRIMARY KEY CLUSTERED ([MessageId] ASC)
                        );

                        CREATE INDEX [IX_ChatHistoryMessages_Conversation_Timestamp]
                            ON dbo.ChatHistoryMessages ([ConversationId] ASC, [TimestampUtc] ASC);

                        CREATE INDEX [IX_ChatHistoryMessages_Session_Timestamp]
                            ON dbo.ChatHistoryMessages ([SessionId] ASC, [TimestampUtc] ASC);
                    END;
                    """
            )
    ];
}