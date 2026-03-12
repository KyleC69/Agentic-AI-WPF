# Copilot Instructions for RAGDataIngestionWPF - *COPILOT* DO NOT MODIFY FILE

## Big picture
- WPF desktop app generated from Template Studio; uses Generic Host for DI/startup and MahApps.Metro for theming; targets `net10.0-windows10.0` (Preview SDK required).
- Solution projects: `RAGDataIngestionWPF` (UI) and `RAGDataIngestionWPF.Core` (services/models); tests in `RAGDataIngestionWPF.Tests.MSTest`.
- Keep `RAGDataIngestionWPF` UI project clean: non-UI functionality must live in a library project; UI should only orchestrate via DI and library service calls, while `RAGDataIngestionWPF.Core` remains UI infrastructure.

## Startup & DI
- `App.xaml.cs` builds `Host` and registers services/pages; activation via `ApplicationHostService` (`IHostedService`).
- Toast activation handled by `ToastNotificationActivationHandler`; defaults to showing `ShellWindow` and navigating to `MainViewModel`.
- Add new services/pages/viewmodels by registering in `ConfigureServices` **and** `PageService.Configure` (key is `ViewModel` `FullName`).
- Keep service registrations organized by feature/area (e.g. Navigation, Theming, Data) and use extension methods for large groups of related services.

## Navigation & UI patterns
- Navigation uses `NavigationService` + `PageService`; pages resolved from DI. `INavigationAware` hooks for `OnNavigatedTo/From` (called in `NavigationService`).
- `ShellWindow` hosts `Frame`; call `INavigationService.Initialize(frame)` before navigation. Use `NavigateTo(typeof(VM).FullName, param, clearNavigation)`; `clearNavigation` triggers `FrameExtensions.CleanNavigation`.
- Themes: `ThemeSelectorService` uses MahApps `ThemeManager`; stores current theme in `App.Current.Properties["Theme"]`. High-contrast themes declared in `Styles/Themes`.
- Ensure that UI elements colors/styles use DynamicResource for theming support (e.g. `{DynamicResource MahApps.Brushes.ThemeBackground}`).
- Ensure UI text is stored in `Properties/Resources.resx` for localization; use `Properties.Resources.ResourceKey` in XAML and code.
- UI responsiveness: use async/await for long-running operations; consider `IAsyncCommand` pattern for commands that trigger async work. Avoid blocking the UI thread.
- UI styles and control templates should be defined in XAML resource dictionaries (e.g. `Styles/Controls.xaml`) and referenced via StaticResource or DynamicResource as appropriate.

## Data & persistence
- App settings bound registry through settings UI.
- `PersistAndRestoreService` REMOVE -- ALL settings are to be contained in UI and configurable from SettingsPage in UI and persisted to System registry. No file-based settings or user-level settings allowed. -- Use DPAPI for any sensitive data that must be stored, and ensure all persisted data is encrypted and stored securely.
- NEED TO REMOVE: `UserDataService` loads cached `User.json` (or defaults to local user) and raises `UserDataUpdated`; uses `ImageHelper` for Base64/asset images. -- REMOVE NO USER LEVEL SETTINGS ALLOWED
- NEED TO REMOVE: `IdentityService` is stubbed (no real auth). `IdentityCacheService` persists MSAL tokens via DPAPI in `%LocalAppData%/RAGDataIngestionWPF/.msalcache.bin3`. -- REMOVE NO USER LEVEL SETTINGS ALLOWED
-- User identity is only needed for key in Chat History database storage and retieval.

## External integrations
- There are to be no external intergrations or dependencies in this project. All code should be self contained and not rely on any external services or APIs. Application should operate in an air-gapped environment, private network only.
- Notifications: `ToastNotificationsService` uses `Microsoft.Toolkit.Uwp.Notifications`; sample toast shown on startup.

## Tests & tooling
- Tests use MSTest latest version. 
- Unit test should focus on behavior and edge cases, not just code coverage. Use `InternalsVisibleTo` for testing internal methods when necessary, but prefer testing through public APIs.
- Build with `dotnet build RAGDataIngestionWPF.sln` (requires Windows + matching preview SDK for .NET 10).

## Common edit points
- Adding a page: create `ViewModel` + `Page`, register both in DI (`App.ConfigureServices`) and map in `PageService`. Use `INavigationAware` for lifecycle events.
- Keep user-facing strings localizable (`Properties/Resources.resx`).

## Debugging tips
- If navigation does nothing, verify `NavigationService.Initialize` called and key matches `ViewModel.FullName`.
- Startup issues often stem from missing DI registrations or `PageService` mappings; replicate test host configuration (`PagesTests`) to compare.

## Specific to RAG and AI Context Handling

- Context injection (ChatHistory and Remote Knowledge RAG store) uses a custom "ChatRole" enum called "AIChatRole" to mark and track context injected messages. The 2 new Roles are AIContext for chathistory injected as context, and RAGContext for injected context from the RAG system. This allows for better tracking and handling of context messages throughout the system, and avoids overloading the existing ChatMessage properties with additional metadata. **Any** injected messages must use one of these roles and immediatly remove those same messages during the round-trip and not store duplicates or add extra noise in the context. 



## Coding Standards and Best Practices and AI Focus

1. Architectural Structure
   - Enforce the use of modern professional patterns and best practices when reviewing code.
   - Utilize base classes and abstract away helpers and minor code to base classes.
   - Ensure classes are loosely coupled and use interfaces when possible.
   - Avoid using existing code patterns as a model for new code; always prefer modern established architectural designs.
   - Demonstrate and explain preferred patterns instead of accepting anti-pattern coding.

2. Layering and Boundaries
   - Identify and point out domain logic leaking into infrastructure or UI layers.
   - Recommend proper separation of concerns and dependency direction.
   - Identify places where abstractions or interfaces should exist.

3. Maintainability and Extensibility
   - Suggest reorganizations that improve readability and long-term maintainability.
   - Identify overly dense methods, unclear flows, or hidden responsibilities.
   - Recommend splitting responsibilities into smaller, well-defined components.
   - Enforce the proper use of base classes and abstraction when appropriate.

4. Dependency Quality
   - Evaluate coupling and testability.
   - Highlight unnecessary static calls, service locators, or tight dependencies.
   - Recommend dependency injection and inversion of control where appropriate.

5. Production-Grade Expectations
   - Identify missing guard clauses, error boundaries, and robustness issues.
   - Recommend improvements that increase reliability and predictability.
   - Highlight anything that prevents the code from being considered production-ready.

6. Communication Style
   - Make suggestions in a polite manner if a different approach would be more effective.
     - Avoid reaching for the easiest or most obvious solution. Instead, consider long-term implications and architectural integrity of your recommendations.
     - Ensure best practices are followed, even if they require more effort upfront, to promote a maintainable and scalable codebase.
   - Provide actionable recommendations, not vague advice.
     - Point out issues or problems constructively, with brief explanations.

7. Tech Stack Used
   - C# 14.0 VS 2026 Insiders, .NET 10.0
   - SQL Server 2022 Semantic Search/Full Text Search -- Temporal database design patterns in API ingestion and Documentation ingestion pipelines.
   - Tables enforce semantic ID and Versioning for all ingested data, focusing on immutability and append-only patterns.
   - RAG system incorporates a hybrid search approach, utilizing vector search, semantic search, and full text search. RAG uses remote knowledge bases with local indexing and caching for performance and reliability. Due to rapidly evolving AI frameworks and tools, local harvest is not practical, but local indexing and caching is critical for performance and reliability. EF Core 10.0 is used for data access, focusing on efficient querying and proper use of DbContext lifetimes to ensure performance and scalability. Some ADO exists and is being replaced when time permits.
   - Unit testing will be done with MS Test Framework, focusing on high code coverage and edge cases. Private methods are exposed for testing through the use of InternalsVisibleTo attribute and careful design of internal APIs. Direct testing of methods is preferred, and failed tests should also be considered successful when testing for expected error conditions; both positive and negative test cases should be included to ensure robustness and reliability of the codebase.








