namespace Chuech.ProjectSce.Core.API.Features.Users;

public class MTRequestAuthenticationContextAccessor
{
    public RequestAuthenticationContext? AuthenticationContext { get; private set; }
    internal void InitializeAuthenticationContext(RequestAuthenticationContext context)
    {
        if (AuthenticationContext is not null)
        {
            throw new InvalidOperationException("The authentication context has already been initialized.");
        }
        AuthenticationContext = context;
    }
}