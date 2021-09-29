using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Authorization;

public abstract class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    protected AuthorizationBehavior(IAuthenticationService authenticationService,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        AuthenticationService = authenticationService;
        Logger = logger;
    }

    protected ILogger<AuthorizationBehavior<TRequest, TResponse>> Logger { get; }
    protected IAuthenticationService AuthenticationService { get; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var userId = AuthenticationService.GetUserIdOrNull();
        if (userId is not { } actualUserId)
        {
            throw new UnauthenticatedException();
        }

        var result = await AuthorizeAsync(request, actualUserId, cancellationToken);
        result.ThrowIfUnsuccessful();

        return await next();
    }

    protected abstract ValueTask<AuthorizationResult> AuthorizeAsync(TRequest request, int userId,
        CancellationToken cancellationToken);
}
