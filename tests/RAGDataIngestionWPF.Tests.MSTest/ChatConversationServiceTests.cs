// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ChatConversationServiceTests.cs
// Author: GitHub Copilot

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ChatConversationServiceTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static TokenBudget MakeBudget(
        int sessionBudget = 100_000,
        int maximumContext = 80_000,
        int systemBudget = 5_000,
        int ragBudget = 10_000,
        int toolBudget = 5_000,
        int metaBudget = 1_000)
        => new()
        {
            SessionBudget = sessionBudget,
            MaximumContext = maximumContext,
            SystemBudget = systemBudget,
            RAGBudget = ragBudget,
            ToolBudget = toolBudget,
            MetaBudget = metaBudget,
            BudgetTotal = sessionBudget + systemBudget + ragBudget + toolBudget + metaBudget
        };

    private static Mock<IAppSettings> MakeSettingsMock(
        string applicationId = "test-app",
        TokenBudget budget = null)
    {
        var mock = new Mock<IAppSettings>();
        mock.SetupGet(s => s.ApplicationId).Returns(applicationId);
        mock.Setup(s => s.GetTokenBudget()).Returns(budget ?? MakeBudget());
        return mock;
    }

    private static ChatConversationService CreateService(
        IAgentFactory agentFactory = null,
        IAppSettings settings = null)
    {
        agentFactory = agentFactory ?? new Mock<IAgentFactory>().Object;
        settings = settings ?? MakeSettingsMock().Object;
        var identityService = new Mock<IHistoryIdentityService>().Object;
        return new ChatConversationService(NullLoggerFactory.Instance, agentFactory, settings, identityService);
    }

    // -------------------------------------------------------------------------
    // Constructor guard tests
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ConstructorWithNullLoggerFactoryThrowsArgumentNullException()
    {
        var agentFactory = new Mock<IAgentFactory>().Object;
        var settings = MakeSettingsMock().Object;
        var identityService = new Mock<IHistoryIdentityService>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = new ChatConversationService(null!, agentFactory, settings, identityService));
    }

    [TestMethod]
    public void ConstructorWithNullAgentFactoryThrowsArgumentNullException()
    {
        var settings = MakeSettingsMock().Object;
        var identityService = new Mock<IHistoryIdentityService>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = new ChatConversationService(NullLoggerFactory.Instance, null!, settings, identityService));
    }

    // -------------------------------------------------------------------------
    // Initial state / property tests
    // -------------------------------------------------------------------------

    [TestMethod]
    public void InitializedDefaultsToFalse()
    {
        var service = CreateService();

        Assert.IsFalse(service.Initialized);
    }

    [TestMethod]
    public void ConversationIdDefaultsToEmptyString()
    {
        var service = CreateService();

        Assert.AreEqual(string.Empty, service.ConversationId);
    }

    [TestMethod]
    public void AIHistoryIsEmptyOnCreation()
    {
        var service = CreateService();

        Assert.AreEqual(0, service.AIHistory.Count);
    }

    [TestMethod]
    public void AllTokenCountsDefaultToZero()
    {
        var service = CreateService();

        Assert.AreEqual(0, service.ContextTokenCount);
        Assert.AreEqual(0, service.SessionTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
    }

    [TestMethod]
    public void ApplicationIdDelegatesToAppSettings()
    {
        var settings = MakeSettingsMock(applicationId: "my-enterprise-app");
        var service = CreateService(settings: settings.Object);

        Assert.AreEqual("my-enterprise-app", service.ApplicationId);
    }

    // -------------------------------------------------------------------------
    // SendRequestToModelAsync – argument / state validation
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task SendRequestWithEmptyContentThrowsArgumentException()
    {
        var service = CreateService();
        service.Initialized = true; // bypass InitializeAsync so _agent remains null

        try
        {
            await service.SendRequestToModelAsync(string.Empty, CancellationToken.None);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException) { }
    }

    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    public async Task SendRequestWithWhitespaceContentThrowsArgumentException(string content)
    {
        var service = CreateService();
        service.Initialized = true;

        try
        {
            await service.SendRequestToModelAsync(content, CancellationToken.None);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException) { }
    }

    [TestMethod]
    public async Task SendRequestWhenAgentNotInitializedThrowsInvalidOperationException()
    {
        // Initialized = true means InitializeAsync() is skipped, so _agent stays null.
        var service = CreateService();
        service.Initialized = true;

        try
        {
            await service.SendRequestToModelAsync("hello", CancellationToken.None);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException) { }
    }

    // -------------------------------------------------------------------------
    // BusyStateChanged event behaviour
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task BusyStateChangedFiresTrueThenFalseEvenWhenCallThrows()
    {
        var service = CreateService();
        service.Initialized = true; // _agent null → InvalidOperationException

        var events = new List<bool>();
        service.BusyStateChanged += (_, busy) => events.Add(busy);

        try
        {
            await service.SendRequestToModelAsync("hello", CancellationToken.None);
        }
        catch (InvalidOperationException) { }

        Assert.AreEqual(2, events.Count);
        Assert.IsTrue(events[0], "First event should signal busy=true");
        Assert.IsFalse(events[1], "Second event should signal busy=false");
    }

    [TestMethod]
    public async Task BusyStateChangedFiresTrueThenFalseWhenContentEmpty()
    {
        var service = CreateService();
        service.Initialized = true;

        var events = new List<bool>();
        service.BusyStateChanged += (_, busy) => events.Add(busy);

        try
        {
            await service.SendRequestToModelAsync(string.Empty, CancellationToken.None);
        }
        catch (ArgumentException) { }

        Assert.AreEqual(2, events.Count);
        Assert.IsTrue(events[0]);
        Assert.IsFalse(events[1]);
    }

    // -------------------------------------------------------------------------
    // Token counting – history-based bucket categorisation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Triggers UpdateTokenCounts via the finally block of a faulted SendRequest call.
    /// </summary>
    private static async Task TriggerTokenUpdateAsync(ChatConversationService service)
    {
        try
        {
            await service.SendRequestToModelAsync("ping", CancellationToken.None);
        }
        catch (InvalidOperationException) { }
    }

    [TestMethod]
    public async Task ContextTokenCountReflectsTotalHistoryTokens()
    {
        var service = CreateService();
        service.Initialized = true;

        // "abcd" = 4 chars → 1 token; add three messages = 3 tokens total
        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd"));
        service.AIHistory.Add(new ChatMessage(ChatRole.Assistant, "abcd"));
        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd"));

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(3, service.ContextTokenCount);
    }

    [TestMethod]
    public async Task UserAndAssistantMessagesCountAsSesionTokens()
    {
        var service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd"));       // 1 token
        service.AIHistory.Add(new ChatMessage(ChatRole.Assistant, "abcd"));  // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(2, service.SessionTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
    }

    [TestMethod]
    public async Task SystemRoleMessageCountsAsSystemTokens()
    {
        var service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(new ChatRole(AIChatRole.System.Value), "abcd")); // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(1, service.SystemTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
        Assert.AreEqual(0, service.SessionTokenCount);
    }

    [TestMethod]
    public async Task ToolRoleMessageCountsAsToolTokens()
    {
        var service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(new ChatRole(AIChatRole.Tool.Value), "abcd")); // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(1, service.ToolTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.SessionTokenCount);
    }

    [TestMethod]
    [DataRow("rag_context")]
    [DataRow("context")]
    [DataRow("rag")]
    public async Task RagRoleVariantsCountAsRagTokens(string roleValue)
    {
        var service = CreateService();
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(new ChatRole(roleValue), "abcd")); // 1 token

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(1, service.RagTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
        Assert.AreEqual(0, service.SessionTokenCount);
    }

    [TestMethod]
    public async Task MessagesExceedingSessionBudgetAreNotCounted()
    {
        // Budget of 2 tokens; add three 1-token messages. The oldest (index 0) should
        // be excluded because iterating from newest: msg[2] (total=1), msg[1] (total=2),
        // msg[0] would make total=3 > 2 so it breaks.
        var budget = MakeBudget(sessionBudget: 2, maximumContext: 999_999);
        var settings = MakeSettingsMock(budget: budget);
        var service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // oldest
        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd"));
        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // newest

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(2, service.ContextTokenCount);
    }

    [TestMethod]
    public async Task OversizedNewestMessageIsClippedToRemainingSlidingWindowBudget()
    {
        // One 12-char message estimates as 3 tokens. Session budget is 2, so the newest
        // message should contribute 2 tokens instead of dropping to zero.
        var budget = MakeBudget(sessionBudget: 2, maximumContext: 999_999);
        var settings = MakeSettingsMock(budget: budget);
        var service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcdefghijkl"));

        await TriggerTokenUpdateAsync(service);

        Assert.AreEqual(2, service.ContextTokenCount);
        Assert.AreEqual(2, service.SessionTokenCount);
        Assert.AreEqual(0, service.RagTokenCount);
        Assert.AreEqual(0, service.ToolTokenCount);
        Assert.AreEqual(0, service.SystemTokenCount);
    }

    // -------------------------------------------------------------------------
    // Budget event tests
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task SessionBugetExceededFiresWhenContextTokensReachSessionBudget()
    {
        // SessionBudget = 1: a single 4-char message (1 token) fills the budget exactly.
        var budget = MakeBudget(sessionBudget: 1, maximumContext: 999_999);
        var settings = MakeSettingsMock(budget: budget);
        var service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        bool fired = false;
        service.SessionBugetExceeded += (_, _) => fired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsTrue(fired, "SessionBugetExceeded should fire when context == SessionBudget");
    }

    [TestMethod]
    public async Task TokenBudgetExceededFiresWhenContextTokensReachSessionBudget()
    {
        var budget = MakeBudget(sessionBudget: 1, maximumContext: 999_999);
        var settings = MakeSettingsMock(budget: budget);
        var service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        bool fired = false;
        service.TokenBudgetExceeded += (_, _) => fired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsTrue(fired, "TokenBudgetExceeded should fire when context == SessionBudget");
    }

    [TestMethod]
    public async Task MaximumContextWarningFiresWhenContextTokensReachMaximumContext()
    {
        // MaximumContext = 1, SessionBudget >> 1: triggers warning but not exceeded.
        var budget = MakeBudget(sessionBudget: 100_000, maximumContext: 1);
        var settings = MakeSettingsMock(budget: budget);
        var service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        int? warningTokenCount = null;
        service.MaximumContextWarning += (_, count) => warningTokenCount = count;

        await TriggerTokenUpdateAsync(service);

        Assert.IsNotNull(warningTokenCount, "MaximumContextWarning should have fired");
        Assert.AreEqual(1, warningTokenCount);
    }

    [TestMethod]
    public async Task MaximumContextWarningDoesNotFireWhenBudgetExceeded()
    {
        // When SessionBudget is also hit, SessionBugetExceeded fires and returns early
        // before MaximumContextWarning can be raised.
        var budget = MakeBudget(sessionBudget: 1, maximumContext: 1);
        var settings = MakeSettingsMock(budget: budget);
        var service = CreateService(settings: settings.Object);
        service.Initialized = true;

        service.AIHistory.Add(new ChatMessage(ChatRole.User, "abcd")); // 1 token

        bool warningFired = false;
        service.MaximumContextWarning += (_, _) => warningFired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsFalse(warningFired,
            "MaximumContextWarning should NOT fire when SessionBugetExceeded path short-circuits");
    }

    [TestMethod]
    public async Task NoBudgetEventsFireWhenHistoryIsEmpty()
    {
        var service = CreateService();
        service.Initialized = true;

        bool anyFired = false;
        service.SessionBugetExceeded += (_, _) => anyFired = true;
        service.TokenBudgetExceeded += (_, _) => anyFired = true;
        service.MaximumContextWarning += (_, _) => anyFired = true;

        await TriggerTokenUpdateAsync(service);

        Assert.IsFalse(anyFired, "No budget events should fire for an empty history");
    }
}
