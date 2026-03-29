---

name: Application Components
description: high level view of UI pages in application
author: Kyle Crowder
status: Work in progress

---

# Application Components - work in progress

## Application features several different components

### Data Ingestion: (DataGrid.xaml) DataIngestionLib/DocIngestion

- Features data ingestion from varying sources into local sql database.
- Features data grid of current chat history contents (currently for dev)

### Main page: ChatConversationService

- Agentic Chat capable of system analysis - Read-only for now, but will be editable in the future.
- Includes several tool for system monitoring - Read-only for now, but will be editable in the future.
- Visible token usage and cost tracking for agent conversations. - may move or make it selectable in the future.
- Chat history persisted to SQL server.
- AIContextInjector implementation to use chat history from any conversation, can be restricted by ApplicationID, UserId, AgentID and date or time.
- AIContextInjector for local sql rag context enrichment.

### Settings page: (Appsettings) SettingsPage.xaml

- All library settings adjustable from Settings UI
- Any UI settings exposed here.

### System Monitoring

- System monitoring tools currently in the main page, will be moved to their own page in the future.
