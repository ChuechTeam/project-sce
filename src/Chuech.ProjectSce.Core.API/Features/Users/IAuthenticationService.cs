using System.Security.Claims;

namespace Chuech.ProjectSce.Core.API.Features.Users;

public interface IAuthenticationService
{
    int GetUserId();
    int? GetUserIdOrNull();
}

public class HttpAuthenticationService : IAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpAuthenticationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetUserIdOrNull()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            return GetHttpUserId(httpContext.User);
        }
        
        throw new InvalidOperationException(
            "Cannot retrieve the current user as there is no active HttpContext.");
    }

    public int GetUserId()
    {
        return GetUserIdOrNull() ?? throw new InvalidOperationException("The user id is null.");
    }

    private static int? GetHttpUserId(ClaimsPrincipal? claimsPrincipal)
    {
        var value = claimsPrincipal?.Claims.FirstOrDefault(x => x.Type == "public_id")?.Value;
        return value == null ? null : int.Parse(value);
    }
}