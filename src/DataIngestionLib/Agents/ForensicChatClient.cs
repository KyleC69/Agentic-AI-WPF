// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.Runtime.CompilerServices;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;





public sealed class ForensicChatClient : DelegatingChatClient
{
    private readonly ILogger<ForensicChatClient> _logger;








    public ForensicChatClient(IChatClient inner, ILogger<ForensicChatClient> logger) : base(inner)
    {
        _logger = logger;
    }








    // --------------------------------------------------------------------
    // NON-STREAMING: GetResponseAsync(messages)
    // --------------------------------------------------------------------








    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var id = CorrelationId();
        this.SafeLog(LogLevel.Information, "[Forensic:{Id}] ENTER GetResponseAsync(messages)", id);

        List<string> anomalies = new();
        this.DetectRequest(messages, "GetResponseAsync(messages)", anomalies);

        ChatResponse response = await base.GetResponseAsync(messages, options, cancellationToken);

        this.DetectResponse(response, "GetResponseAsync(messages)", anomalies);

        this.LogAnomalies(id, anomalies);
        this.SafeLog(LogLevel.Information, "[Forensic:{Id}] EXIT GetResponseAsync(messages)", id);

        return response;
    }








    // --------------------------------------------------------------------
    // STREAMING: GetStreamingResponseAsync(messages)
    // --------------------------------------------------------------------








    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var id = CorrelationId();
        this.SafeLog(LogLevel.Information, "[Forensic:{Id}] ENTER GetStreamingResponseAsync(messages)", id);

        List<string> anomalies = new();
        this.DetectRequest(messages, "GetStreamingResponseAsync(messages)", anomalies);

        var sawAny = false;

        await foreach (ChatResponseUpdate update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            sawAny = true;
            this.DetectStreaming(update, "GetStreamingResponseAsync(messages)", anomalies);
            yield return update;
        }

        if (!sawAny)
        {
            anomalies.Add("No streaming updates observed.");
        }

        this.LogAnomalies(id, anomalies);
        this.SafeLog(LogLevel.Information, "[Forensic:{Id}] EXIT GetStreamingResponseAsync(messages)", id);
    }








    private static string CorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }








    // --------------------------------------------------------------------
    // REQUEST ANOMALY DETECTION
    // --------------------------------------------------------------------








    private void DetectRequest(IEnumerable<ChatMessage> messages, string method, List<string> anomalies)
    {
        if (messages == null)
        {
            anomalies.Add($"{method}: messages is null.");
            return;
        }

        IList<ChatMessage> messageList = messages as IList<ChatMessage> ?? messages.ToList();
        if (!messageList.Any())
        {
            anomalies.Add($"{method}: messages is empty.");
        }

        if (messageList.Any(message => message == null))
        {
            anomalies.Add($"{method}: null ChatMessage encountered.");
        }

        if (messageList.Any(message => message?.Role == null))
        {
            anomalies.Add($"{method}: message with null Role.");
        }

        if (messageList.Any(message => message?.Contents == null))
        {
            anomalies.Add($"{method}: message with null Content.");
        }
    }








    // --------------------------------------------------------------------
    // RESPONSE ANOMALY DETECTION
    // --------------------------------------------------------------------








    private void DetectResponse(ChatResponse response, string method, List<string> anomalies)
    {
        if (response == null)
        {
            anomalies.Add($"{method}: response is null.");
            return;
        }

        if (response.Messages == null || !response.Messages.Any())
        {
            anomalies.Add($"{method}: response.Messages is null or empty (synthetic/fallback).");
        }

        if (response.Usage == null)
        {
            anomalies.Add($"{method}: response.Usage is null (missing token usage).");
        }

        if (response.ConversationId == null)
        {
            anomalies.Add($"{method}: response.ConversationId is null (synthetic response).");
        }

        if (response.Messages?.Count() == 1)
        {
            anomalies.Add($"{method}: single assistant message (fallback signature).");
        }
    }








    // --------------------------------------------------------------------
    // STREAMING ANOMALY DETECTION
    // --------------------------------------------------------------------








    private void DetectStreaming(ChatResponseUpdate update, string method, List<string> anomalies)
    {
        if (update == null)
        {
            anomalies.Add($"{method}: null streaming update.");
            return;
        }

        if (update.Contents == null && update.MessageId == null)
        {
            anomalies.Add($"{method}: streaming update contains neither Contents nor MessageId.");
        }
    }








    private void LogAnomalies(string id, List<string> anomalies)
    {
        if (anomalies.Count == 0)
        {
            return;
        }

        this.SafeLog(LogLevel.Warning, "[Forensic:{Id}] Anomalies detected:\n - {A}", id, string.Join("\n - ", anomalies));
    }








    private void SafeLog(LogLevel level, string message, params object?[] args)
    {
        try
        {
            _logger.Log(level, message, args);
        }
        catch
        {
            /* swallow */
        }
    }
}