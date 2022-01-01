using Chuech.ProjectSce.Core.API.Features.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public sealed class UserSpaceMember : SpaceMember
{
    private UserSpaceMember() : base()
    {
    }

    public UserSpaceMember(Space space, int userId, SpaceMemberCategory category) : base(space, category)
    {
        UserId = userId;
    }

    public User User { get; set; } = null!;
    public int UserId { get; set; }

    internal class Configration : IEntityTypeConfiguration<UserSpaceMember>
    {
        public void Configure(EntityTypeBuilder<UserSpaceMember> builder)
        {
            // Note: In postgres, this works as intended without needing a filter for NULL values.
            builder.HasIndex(x => new { x.SpaceId, x.UserId })
                .IsUnique();
        }
    }
}