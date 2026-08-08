using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Core.Agents.Common.Middleware;

public sealed class FunctionLoggingMiddleware(ILoggerFactory loggerFactory)
{
    readonly ILogger<FunctionLoggingMiddleware> _logger = loggerFactory.CreateLogger<FunctionLoggingMiddleware>();

    public async ValueTask<object?> InvokeAsync(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Called Tool {Name} | args: {Args}",
            context.Function.Name,
            JsonSerializer.Serialize(context.Arguments));

        var result = await next(context, cancellationToken);

        _logger.LogInformation("Tool {Name} finished in {Ms}ms | Response: {Result}", context.Function.Name, stopwatch.ElapsedMilliseconds, result);

        return result;
    }
}
