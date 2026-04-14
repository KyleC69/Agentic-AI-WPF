// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         MainViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 212941



using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using AgentAILib.Agents;
using AgentAILib.Contracts;
using AgentAILib.Models;
using AgentAILib.Services;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using AgenticAIWPF.Contracts.ViewModels;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;




namespace AgenticAIWPF.ViewModels;





public sealed partial class MainViewModel : ObservableObject, IDisposable, INavigationAware
{
    private readonly IChatConversationService _chatConversationService;
    private bool _historyLoaded;
    private CancellationTokenSource _tokenSource;

    [ObservableProperty] private int cachedInputTokenCount;
    [ObservableProperty] private int inputTokenCount;
    [ObservableProperty] private int outputTokenCount;

    //Running Token counts for different categories
    [ObservableProperty] private int ragTokenCount;
    [ObservableProperty] private int reasoningTokenCount;
    [ObservableProperty] private int sessionTokenCount;
    [ObservableProperty] private int systemTokenCount;
    [ObservableProperty] private int toolTokenCount;
    [ObservableProperty] private int totalTokenCount;
    private readonly IWorkflowConversationService _workflow;
    private readonly ILogger<MainViewModel> _logger;








    public MainViewModel(IChatConversationService chatConversationService, IWorkflowConversationService workflowConversationService, ILogger<MainViewModel> logger)
    {
        Guard.IsNotNull(chatConversationService);
        Guard.IsNotNull(workflowConversationService);
        Guard.IsNotNull(logger);
        _logger = logger;
        _workflow = workflowConversationService;
        _chatConversationService = chatConversationService;
        SendMessageCommand = new AsyncRelayCommand(StartWorkflowAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);
        Messages = new();
        _chatConversationService.BusyStateChanged += OnBusyStateChange;
        _workflow.BusyStateChanged += OnBusyStateChange;

        NewConvoCommand = new AsyncRelayCommand(StartNewConversationAsync);

        // Need to link this back to applicaion lifecycle
        _tokenSource = new CancellationTokenSource();
        TokenAccountingMiddleware.TotalTokensChanged += OnTotalTokensChanged;
        TokenAccountingMiddleware.RagTokensChanged += OnRagTokensChanged;
        TokenAccountingMiddleware.ReasoningTokensChanged += OnReasoningTokensChanged;
        TokenAccountingMiddleware.InputTokensChanged += OnInputTokensChanged;
        TokenAccountingMiddleware.OutputTokensChanged += OnOutputTokensChanged;
        TokenAccountingMiddleware.CachedInputTokensChanged += OnCachedInputTokensChanged;
        TokenAccountingMiddleware.SessionTokensChanged += OnSessionTokensChanged;
        TokenAccountingMiddleware.ToolTokensChanged += OnToolTokensChanged;
        TokenAccountingMiddleware.SystemTokensChanged += OnSystemTokensChanged;
    }








    public IRelayCommand CancelMessageCommand { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the application is currently in a busy state.
    /// </summary>
    /// <remarks>
    ///     This property is used to manage the application's busy state, which affects the availability of commands
    ///     such as <see cref="SendMessageCommand" /> and <see cref="CancelMessageCommand" />. It also triggers the
    ///     <see cref="OnBusyStateChange" /> method to handle state changes.
    /// </remarks>
    public bool IsBusy
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string MessageInput
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
            }
        }
    } = string.Empty;

    public ObservableCollection<ChatMessage> Messages { get; }
    public IAsyncRelayCommand NewConvoCommand { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }


    public AIModelDescriptor SelectedModel
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value) && value is not null)
            {
                _ = ApplyModelChangeAsync(value);
            }
        }
    } = AIModels.Default;








    private async Task ApplyModelChangeAsync(AIModelDescriptor descriptor)
    {
        try
        {
            await _chatConversationService.ChangeModelAsync(descriptor, _tokenSource.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch active model to {ModelId}.", descriptor.ModelId);
        }
    }








    public void Dispose()
    {
        _chatConversationService.BusyStateChanged -= OnBusyStateChange;
        _workflow.BusyStateChanged -= OnBusyStateChange;

        TokenAccountingMiddleware.TotalTokensChanged -= OnTotalTokensChanged;
        TokenAccountingMiddleware.RagTokensChanged -= OnRagTokensChanged;
        TokenAccountingMiddleware.ReasoningTokensChanged -= OnReasoningTokensChanged;
        TokenAccountingMiddleware.InputTokensChanged -= OnInputTokensChanged;
        TokenAccountingMiddleware.OutputTokensChanged -= OnOutputTokensChanged;
        TokenAccountingMiddleware.CachedInputTokensChanged -= OnCachedInputTokensChanged;
        TokenAccountingMiddleware.SessionTokensChanged -= OnSessionTokensChanged;
        TokenAccountingMiddleware.ToolTokensChanged -= OnToolTokensChanged;
        TokenAccountingMiddleware.SystemTokensChanged -= OnSystemTokensChanged;

        _tokenSource?.Cancel();
        _tokenSource?.Dispose();

    }








    /// <summary>
    ///     Called when the view model is navigated away from.
    /// </summary>
    /// <remarks>
    ///     This method is part of the <see cref="INavigationAware" /> interface and is invoked
    ///     to handle any cleanup or state-saving logic when the associated view is no longer active.
    /// </remarks>
    public void OnNavigatedFrom()
    {
    }








    /// <summary>
    ///     Called when the view model is navigated to.
    /// </summary>
    /// <param name="parameter">
    ///     An optional parameter passed during navigation. This can be used to provide
    ///     context or data required by the view model.
    /// </param>
    /// <remarks>
    ///     This method is part of the <see cref="INavigationAware" /> interface and is invoked
    ///     to handle initialization logic, such as loading data or setting up state, when the
    ///     associated view becomes active.
    /// </remarks>
    public async void OnNavigatedTo(object parameter)
    {

        if (_historyLoaded)
        {
            return;
        }


        // Load conversation history on navigation

        try
        {
            await LoadConversationHistoryAsync().ConfigureAwait(false);

            _historyLoaded = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load conversation history.");
        }








    }








    private async Task LoadConversationHistoryAsync()
    {
        try
        {
            // Ensure a new token source is created for this operation if needed, or reuse if appropriate.
            // For simplicity here, we assume it might be okay to use a potentially cancelled _tokenSource if the UI is already being navigated away from.
            // A more robust approach might create a new CancellationTokenSource specifically for loading history.
            var history = await _chatConversationService.LoadConversationHistoryAsync(_tokenSource.Token).ConfigureAwait(false);
            foreach (ChatMessage chatMessage in history)
            {
                Messages.Add(chatMessage);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Loading conversation history was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load conversation history.");
            // Optionally, inform the user or handle the error in a way that doesn't break the UI.
        }

    }








    private bool CanCancelMessage()
    {
        return IsBusy;
    }








    private void CancelMessage()
    {
        _tokenSource?.Cancel();
    }








    private bool CanSendMessage()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(MessageInput);
    }








    private void OnBusyStateChange(object sender, bool e)
    {
        RunOnUiThread(() => IsBusy = e);
    }








    private static void RunOnUiThread(Action updateAction)
    {
        ArgumentNullException.ThrowIfNull(updateAction);

        Dispatcher dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            updateAction();
            return;
        }

        _ = dispatcher.InvokeAsync(updateAction);
    }








    private async Task StartWorkflowAsync()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _tokenSource = new CancellationTokenSource();
        IsBusy = true;
        var content = MessageInput.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            IsBusy = false;
            return;
        }

        //Add Users message to UI collection
        Messages.Add(new ChatMessage(ChatRole.User, content));

        //Clear UI input
        MessageInput = string.Empty;
        try
        {
            //  await _workflow.InitializeAsync().ConfigureAwait(false);
            //  var result = await _workflow.ExecuteWorkflow(content).ConfigureAwait(true);

            var result = await _chatConversationService.SendRequestToModelAsync(content, _tokenSource.Token);
            result = PreprocessMessage(result);
            Messages.Add(result);
            //  Messages.Add(new ChatMessage(ChatRole.Assistant, result));
        }
        catch (OperationCanceledException e)
        {
            _logger.LogError(e, "Error running workflow");
            Messages.Add(new ChatMessage(ChatRole.Assistant, $"Error running workflow: {e.Message}"));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running workflow");
            Messages.Add(new ChatMessage(ChatRole.Assistant, $"Error running workflow: {ex.Message}"));
        }

    }








    /// <summary>
    /// Searchs for code blocks and wraps with copy controls
    /// </summary>
    /// <param name="result"></param>
    private static ChatMessage PreprocessMessage(ChatMessage original)
    {
        Guard.IsNotNull(original);

        var originalText = original.Text ?? string.Empty;
        var rewrittenText = Regex.Replace(
            originalText,
            @"(?ms)^```[^\r\n]*\r?\n[\s\S]*?^```[ \t]*(?=\r?\n|$)",
            static match => WrapCodeBlock(match.Value));

        ChatMessage rewrittenMessage = new(original.Role, rewrittenText)
        {
            AuthorName = original.AuthorName,
            CreatedAt = original.CreatedAt,
            MessageId = original.MessageId,
            RawRepresentation = original.RawRepresentation
        };

        if (original.AdditionalProperties is not null)
        {
            rewrittenMessage.AdditionalProperties = [];
            foreach (var property in original.AdditionalProperties)
            {
                rewrittenMessage.AdditionalProperties[property.Key] = property.Value;
            }
        }

        return rewrittenMessage;

        static string WrapCodeBlock(string fencedBlock)
        {
            string codeToCopy = ExtractCodeContent(fencedBlock);
            string encodedCode = WebUtility.HtmlEncode(codeToCopy);
            StringBuilder builder = new();
            _ = builder.AppendLine("<div class=\"chat-code-block\">");
            _ = builder.AppendLine($"<button class=\"chat-code-copy-button\" data-copy=\"{encodedCode}\">Copy</button>");
            _ = builder.AppendLine();
            _ = builder.Append(fencedBlock);
            _ = builder.AppendLine();
            _ = builder.Append("</div>");
            return builder.ToString();
        }

        static string ExtractCodeContent(string fencedBlock)
        {
            string normalizedBlock = fencedBlock.Replace("\r\n", "\n");
            string[] lines = normalizedBlock.Split('\n');
            if (lines.Length <= 2)
            {
                return string.Empty;
            }

            return string.Join(Environment.NewLine, lines.Skip(1).Take(lines.Length - 2));
        }
    }















    private async Task StartNewConversationAsync(CancellationToken arg)
    {
        // Clear the current conversation in the service, which should trigger the UI to clear as well.
        await _chatConversationService.StartNewConversationAsync(arg).ConfigureAwait(false);
        Messages.Clear();
    }






    private void OnCachedInputTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => CachedInputTokenCount = e.CurrentValue);
    }






    private void OnInputTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => InputTokenCount = e.CurrentValue);
    }






    private void OnOutputTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => OutputTokenCount = e.CurrentValue);
    }






    private void OnRagTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => RagTokenCount = e.CurrentValue);
    }






    private void OnReasoningTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => ReasoningTokenCount = e.CurrentValue);
    }






    private void OnSessionTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => SessionTokenCount = e.CurrentValue);
    }






    private void OnSystemTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => SystemTokenCount = e.CurrentValue);
    }






    private void OnToolTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => ToolTokenCount = e.CurrentValue);
    }






    private void OnTotalTokensChanged(object sender, TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RunOnUiThread(() => TotalTokenCount = e.CurrentValue);
    }
}