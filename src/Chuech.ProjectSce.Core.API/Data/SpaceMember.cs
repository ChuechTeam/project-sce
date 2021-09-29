using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Data;

public abstract class SpaceMember
{
    protected SpaceMember()
    {
        Space = null!;
    }

    protected SpaceMember(Space space, SpaceMemberCategory category)
    {
        Space = space;
        SpaceId = space.Id;
        Category = category;
    }

    public int Id { get; set; }

    public Space Space { get; set; }
    public int SpaceId { get; set; }

    public SpaceMemberCategory Category { get; set; }
}
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
public sealed class GroupSpaceMember : SpaceMember
{
    private GroupSpaceMember() : base()
    {
    }

    public GroupSpaceMember(Space space, int groupId, SpaceMemberCategory category) : base(space, category)
    {
        GroupId = groupId;
    }

    public Group Group { get; set; } = null!;
    public int GroupId { get; set; }

    internal class Configration : IEntityTypeConfiguration<GroupSpaceMember>
    {
        public void Configure(EntityTypeBuilder<GroupSpaceMember> builder)
        {   
            builder.HasIndex(x => new { x.SpaceId, x.GroupId })
                .IsUnique();
        }
    }
}