using Core.Agents.Common;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared.Domain.Services;

namespace Core.Agents.Chat;

public sealed class DaprChatHistoryProvider(ICacheService cacheService, Guid sessionId) : ChatHistoryProvider
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly string _key = ChatCacheKeyReference.Messages(sessionId);

    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        var messages = await _cacheService.GetAsync<IEnumerable<ChatMessage>>(_key, cancellationToken) ?? [];
        return messages;
    }

    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
        await _cacheService.SetAsync(_key, allNewMessages, TimeSpan.FromMinutes(10), cancellationToken);
        return;
    }
}
