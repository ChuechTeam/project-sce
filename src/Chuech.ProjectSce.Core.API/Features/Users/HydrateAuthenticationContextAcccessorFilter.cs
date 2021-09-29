using GreenPipes;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Users;

public class HydrateAuthenticationContextAcccessorFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly MTRequestAuthenticationContextAccessor _accessor;

    public HydrateAuthenticationContextAcccessorFilter(MTRequestAuthenticationContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public void Probe(ProbeContext context)
    {
    }

    public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var authContext = context.Headers.Get<RequestAuthenticationContext>("AuthContext");
        if (authContext is not null)
        {
            _accessor.InitializeAuthenticationContext(authContext);
        }

        return next.Send(context);
    }
}
