using Chuech.ProjectSce.Core.API.Features.Groups;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public sealed class GroupSpaceMember : SpaceMember
{
    private GroupSpaceMember()
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