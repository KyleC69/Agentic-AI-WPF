# Copilot Instructions for RAGDataIngestionWPF

## Big picture
- WPF desktop app generated from Template Studio; uses Generic Host for DI/startup and MahApps.Metro for theming; targets `net10.0-windows10.0` (Preview SDK required).
- Solution projects: `RAGDataIngestionWPF` (UI) and `RAGDataIngestionWPF.Core` (services/models); tests in `RAGDataIngestionWPF.Tests.MSTest`.
- Keep `RAGDataIngestionWPF` UI project clean: non-UI functionality must live in a library project; UI should only orchestrate via DI and library service calls, while `RAGDataIngestionWPF.Core` remains UI infrastructure.

## Startup & DI
- `App.xaml.cs` builds `Host` and registers services/pages; activation via `ApplicationHostService` (`IHostedService`).
- Toast activation handled by `ToastNotificationActivationHandler`; defaults to showing `ShellWindow` and navigating to `MainViewModel`.
- Add new services/pages/viewmodels by registering in `ConfigureServices` **and** `PageService.Configure` (key is `ViewModel` `FullName`).

## Navigation & UI patterns
- Navigation uses `NavigationService` + `PageService`; pages resolved from DI. `INavigationAware` hooks for `OnNavigatedTo/From` (called in `NavigationService`).
- `ShellWindow` hosts `Frame`; call `INavigationService.Initialize(frame)` before navigation. Use `NavigateTo(typeof(VM).FullName, param, clearNavigation)`; `clearNavigation` triggers `FrameExtensions.CleanNavigation`.
- Themes: `ThemeSelectorService` uses MahApps `ThemeManager`; stores current theme in `App.Current.Properties["Theme"]`. High-contrast themes declared in `Styles/Themes`.
- Ensure that UI elements colors/styles use DynamicResource for theming support (e.g. `{DynamicResource MahApps.Brushes.ThemeBackground}`).

## Data & persistence
- App settings bound to `AppConfig` from `appsettings.json` (copied to output). Defaults: `ConfigurationsFolder` under `%LocalAppData%`, `User.json`, `AppProperties.json`, `privacyStatement` URL.
- `PersistAndRestoreService` saves `App.Current.Properties` to local app data via `FileService` JSON serializer.
- `UserDataService` loads cached `User.json` (or defaults to local user) and raises `UserDataUpdated`; uses `ImageHelper` for Base64/asset images.
- `IdentityService` is stubbed (no real auth). `IdentityCacheService` persists MSAL tokens via DPAPI in `%LocalAppData%/RAGDataIngestionWPF/.msalcache.bin3`.

## External integrations
- HttpClient named `msgraph` preconfigured to `https://graph.microsoft.com/v1.0/`. `MicrosoftGraphService` lives in Core; identity is stubbed, so calls likely need wiring.
- Notifications: `ToastNotificationsService` uses `Microsoft.Toolkit.Uwp.Notifications`; sample toast shown on startup.

## Tests & tooling
- Tests use MSTest + Moq. Key coverage: `SettingsViewModelTests`, `PagesTests` (ensures DI and `PageService` mappings). Run with `dotnet test RAGDataIngestionWPF.Tests.MSTest`.
- Build with `dotnet build RAGDataIngestionWPF.sln` (requires Windows + matching preview SDK for .NET 10).

## Common edit points
- Adding a page: create `ViewModel` + `Page`, register both in DI (`App.ConfigureServices`) and map in `PageService`. Use `INavigationAware` for lifecycle events.
- Add new persisted property: update `AppConfig` (appsettings), handle in `PersistAndRestoreService` or services consuming it. Keep file paths under `%LocalAppData%/ConfigurationsFolder`.
- Keep user-facing strings localizable (`Properties/Resources.resx` already used for `AppDisplayName`).

## Debugging tips
- If navigation does nothing, verify `NavigationService.Initialize` called and key matches `ViewModel.FullName`.
- Startup issues often stem from missing DI registrations or `PageService` mappings; replicate test host configuration (`PagesTests`) to compare.

# Expert .NET software engineer mode instructions

You are in expert software engineer mode. Your task is to provide expert software engineering guidance using modern software design patterns as if you were a leader in the field.

1. Architectural Structure
   - When reviewing code, enforce the use of modern day professional patterns and best practices.
   - Be sure to make use of base classes and abstract away helpers and minor code to base classes.
   - Ensure classes are loosely coupled and use interfaces when possible.
   - Don't use the existing code patterns as a model for new code; always prefer modern established architectural designs.
   - Demonstrate and explain the preferred patterns instead of accepting anti-pattern coding.

2. Layering and Boundaries
   - Point out domain logic leaking into infrastructure or UI layers.
   - Recommend proper separation of concerns and dependency direction.
   - Identify places where abstractions or interfaces should exist.

3. Maintainability and Extensibility
   - Suggest reorganizations that improve readability and long-term maintainability.
   - Identify overly dense methods, unclear flows, or hidden responsibilities.
   - Recommend splitting responsibilities into smaller, well-defined components.
   - Enforce user to properly use base classes and abstraction when appropriate.

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
     - Do not reach for the easiest or most obvious solution. Instead, consider the long-term implications and architectural integrity of your recommendations.
     - Ensure best practices are followed, even if they require more effort upfront, to promote a maintainable and scalable codebase.
   - Provide actionable recommendations, not vague advice.
     - Do not be afraid to point out issues or problems; you are encouraged and expected to point out architectural flaws or areas for improvement, but always do so constructively and with brief explanations.

7. Tech Stack Used
   - C# 14.0 VS 2026 Insiders, .NET 10.0
   - SQL Server 2022 Semantic Search/Full Text Search -- Temporal database design patterns in API ingestion and Documentation ingestion pipelines.
   - Tables enforce semantic ID and Versioning for all ingested data, with a focus on immutability and append-only patterns.
   - RAG system incorporates a hybrid search approach, utilizing vector search, semantic search, and full text search. RAG uses remote knowledge bases with local indexing and caching for performance and reliability. Due to the rapidly evolving AI frameworks and tools, local harvest is not practical, but local indexing and caching is critical for performance and reliability. EF Core 10.0 is used for data access, with a focus on efficient querying and proper use of DbContext lifetimes to ensure performance and scalability. Some ADO exists and is being replaced when time permits.
   - Unit testing will be done with MS Test Framework, with a focus on high code coverage and edge cases. Private methods are exposed for testing through the use of InternalsVisibleTo attribute and careful design of internal APIs. Direct testing of methods is preferred, and failed tests should also be considered successful when testing for expected error conditions; both positive and negative test cases should be included to ensure robustness and reliability of the codebase.







