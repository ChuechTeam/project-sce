using GreenPipes;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Users;

public class PassAuthenticationInfoFilter<T> : IFilter<PublishContext<T>> where T : class
{
    private readonly IAuthenticationService _authenticationService;

    public PassAuthenticationInfoFilter(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public void Probe(ProbeContext context)
    {
    }

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (_authenticationService.HasAuthenticationSource)
        {
            context.Headers.Set("AuthContext", 
                new RequestAuthenticationContext(_authenticationService.GetUserIdOrNull()));
        }

        return next.Send(context);
    }
}
