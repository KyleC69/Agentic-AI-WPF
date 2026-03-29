// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         TokenUsageTrackingChatClient.cs
// Author: GitHub Copilot



using Microsoft.Extensions.AI;



namespace DataIngestionLib.Agents;





internal sealed class TokenUsageTrackingChatClient : IChatClient
{
    private readonly IChatClient _innerClient;
    private readonly Action<UsageDetails> _usageSink;






    public TokenUsageTrackingChatClient(IChatClient innerClient, Action<UsageDetails> usageSink)
    {
        ArgumentNullException.ThrowIfNull(innerClient);
        ArgumentNullException.ThrowIfNull(usageSink);

        _innerClient = innerClient;
        _usageSink = usageSink;
    }






    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        ChatResponse response = await _innerClient.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        if (response.Usage is not null)
        {
            _usageSink(response.Usage);
        }

        return response;
    }






    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return _innerClient.GetService(serviceType, serviceKey);
    }






    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        return _innerClient.GetStreamingResponseAsync(messages, options, cancellationToken);
    }






    public void Dispose()
    {
        _innerClient.Dispose();
    }
}