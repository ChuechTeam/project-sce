using Microsoft.AspNetCore.Identity;

namespace Chuech.ProjectSce.Identity.Data
{
    // Note: This does not follow DDD rules because ASP.NET Core Identity works a lot different, and it would
    // be rather useless to create a DDD layer for your DDD pleasure.
    public class UserAccount : IdentityUser
    {
        public UserProfile UserProfile { get; set; } = null!;
        public int UserProfileId { get; set; }
    }
}