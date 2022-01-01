using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Resources;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using Chuech.ProjectSce.Core.API.Features.Subjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Spaces;

public sealed class Space
{
    private Space()
    {
        Subject = null!;
        Institution = null!;
        Name = null!;
    }

    public Space(int institutionId, string name, int subjectId, int managerId)
    {
        Name = name;
        Subject = null!;
        SubjectId = subjectId;
        Institution = null!;
        InstitutionId = institutionId;

        AddUserMember(managerId, SpaceMemberCategory.Manager);
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public Subject Subject { get; private set; }
    public int SubjectId { get; private set; }

    public Institution Institution { get; private set; }
    public int InstitutionId { get; private set; }

    public int ManagerCount { get; private set; }

    private ICollection<SpaceMember> _members = new HashSet<SpaceMember>();
    public IEnumerable<SpaceMember> Members => _members;

    public ICollection<Resource> PublishedResources { get; private set; } = null!;

    public void AddUserMember(int userId, SpaceMemberCategory category)
    {
        if (_members.OfType<UserSpaceMember>().Any(x => x.UserId == userId))
        {
            throw Errors.MemberAlreadyPresent.AsException();
        }
        _members.Add(new UserSpaceMember(this, userId, category));
        UpdateManagerCount();
    }

    public void AddGroupMember(int groupId, SpaceMemberCategory category)
    {
        if (_members.OfType<GroupSpaceMember>().Any(x => x.GroupId == groupId))
        {
            throw Errors.MemberAlreadyPresent.AsException();
        }
        _members.Add(new GroupSpaceMember(this, groupId, category));
        UpdateManagerCount();
    }

    public void RemoveMember(int id, bool allowExceptionalLackOfManagers = false)
    {
        var member = _members.FirstOrDefault(x => x.Id == id);
        if (member is null)
        {
            throw new ArgumentException("Member not found.", nameof(id));
        }
        _members.Remove(member);
        UpdateManagerCount(allowExceptionalLackOfManagers);
    }

    public void UpdateMemberCategory(int id, SpaceMemberCategory category)
    {
        var member = _members.FirstOrDefault(x => x.Id == id);
        if (member is null)
        {
            throw new ArgumentException("Member not found.", nameof(id));
        }
        member.Category = category;
        UpdateManagerCount();
    }

    private void UpdateManagerCount(bool allowExceptionalLackOfManagers = false)
    {
        ManagerCount = _members.Count(x => x is UserSpaceMember && x.Category == SpaceMemberCategory.Manager);
        if (ManagerCount == 0 && !allowExceptionalLackOfManagers)
        {
            throw Errors.LastManager.AsException();
        }
    }

    internal class Configuration : IEntityTypeConfiguration<Space>
    {
        public void Configure(EntityTypeBuilder<Space> builder)
        {
            builder.UseXminAsConcurrencyToken();
            builder.Navigation(x => x.Members).AutoInclude();
        }
    }

    public static class Errors
    {
        public static readonly Error MemberAlreadyPresent = new(
            "The member (user or group) is already present in the space.",
            "space.memberAlreadyPresent");
        public static readonly Error LastManager = new(
            "There would be no managers, as individual users, in the space.", 
            "space.lastManager");
    }
}