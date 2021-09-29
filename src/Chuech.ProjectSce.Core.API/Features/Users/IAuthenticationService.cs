using System.Data;
using System.Security.Claims;

namespace Chuech.ProjectSce.Core.API.Features.Users
{
    public interface IAuthenticationService
    {
        bool HasAuthenticationSource { get; }
        int GetUserId();
        int? GetUserIdOrNull(bool throwOnMissingSource = true);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MTRequestAuthenticationContextAccessor _mtRequestAuthAccessor;

        public AuthenticationService(IHttpContextAccessor httpContextAccessor, MTRequestAuthenticationContextAccessor mtRequestAuthAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _mtRequestAuthAccessor = mtRequestAuthAccessor;
        }

        public bool HasAuthenticationSource => _httpContextAccessor.HttpContext is not null ||
            _mtRequestAuthAccessor.AuthenticationContext is not null;

        public int? GetUserIdOrNull(bool throwOnMissingSource)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                return GetHttpUserId(httpContext.User);
            }

            var requestContext = _mtRequestAuthAccessor.AuthenticationContext;
            if (requestContext is not null)
            {
                return requestContext.UserId;
            }

            if (throwOnMissingSource)
            {
                throw new InvalidOperationException(
                    "Cannot retrieve the current user as there is no active HttpContext " +
                    "or MTRequestAuthenticationContext.");
            }
            else
            {
                return null;
            }
        }

        public int GetUserId()
        {
            return GetUserIdOrNull(true) ?? throw new InvalidOperationException("The user id is null.");
        }

        private int? GetHttpUserId(ClaimsPrincipal? claimsPrincipal)
        {
            var value = claimsPrincipal?.Claims.FirstOrDefault(x => x.Type == "public_id")?.Value;
            return value == null ? null : int.Parse(value);
        }
    }
}