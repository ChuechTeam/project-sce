using MassTransit;

namespace Chuech.ProjectSce.Core.API.Infrastructure;

public static class ConsumeContextResponseExtensions
{
    public static Task RespondIfNeededAsync<T>(this ConsumeContext consumeContext, T message) where T : class
    {
        if (consumeContext.RequestId is not null)
        {
            return consumeContext.RespondAsync(message);
        }
        else
        {
            return Task.CompletedTask;
        }
    }
}