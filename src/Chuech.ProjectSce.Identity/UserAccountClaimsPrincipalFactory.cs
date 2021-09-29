using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chuech.ProjectSce.Identity.Data;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace Chuech.ProjectSce.Identity
{
    public class UserAccountClaimsPrincipalFactory : IUserClaimsPrincipalFactory<UserAccount>
    {
        private readonly IUserClaimsPrincipalFactory<UserAccount> _inner;

        public UserAccountClaimsPrincipalFactory(IUserClaimsPrincipalFactory<UserAccount> inner)
        {
            _inner = inner;
        }

        public async Task<ClaimsPrincipal> CreateAsync(UserAccount user)
        {
            var principal = await _inner.CreateAsync(user);
            var identity = principal.Identities.First();

            identity.AddClaim(new Claim(ChuechClaimTypes.PublicId, user.UserProfileId.ToString()));

            return principal;
        }
    }
}