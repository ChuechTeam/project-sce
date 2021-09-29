
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Authorization;
/// <summary>
/// A helper class to do authentication and authorization at the same time.
/// </summary>
/// <typeparam name="TAuthorizer">The authorizer type</typeparam>
public sealed class AuthBarrier<TAuthorizer>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly TAuthorizer _authorizer;

    public AuthBarrier(IAuthenticationService authenticationService, TAuthorizer authorizer)
    {
        _authenticationService = authenticationService;
        _authorizer = authorizer;
    }

    public async Task<int> GetAuthorizedUserIdAsync(Func<TAuthorizer, int, Task<AuthorizationResult>> authorizeAsync)
    {
        var userId = _authenticationService.GetUserId();

        var authorizationResult = await authorizeAsync(_authorizer, userId);
        authorizationResult.ThrowIfUnsuccessful();

        return userId;
    }

    public async Task<OperationResult<int>> GetAuthorizedUserIdResultAsync(Func<TAuthorizer, int, Task<AuthorizationResult>> authorizeAsync)
    {
        var userId = _authenticationService.GetUserId();

        var authorizationResult = await authorizeAsync(_authorizer, userId);
        if (authorizationResult.IsSuccess)
        {
            return OperationResult.Success(userId);
        }
        else
        {
            return OperationResult.Failure<int>(authorizationResult.AsError()!);
        }
    }
}
