using System;

namespace Chuech.ProjectSce.Identity.Data
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string DisplayName { get; private set; }

        public UserProfile(string displayName)
        {
            DisplayName = displayName;
        }

        public void SetDisplayName(string displayName)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }
    }
}