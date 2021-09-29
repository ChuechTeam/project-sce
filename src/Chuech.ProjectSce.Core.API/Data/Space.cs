using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Data
{
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
        public Subject Subject { get; set; } // For EF stuff
        public int SubjectId { get; private set; }

        public Institution Institution { get; private set; }
        public int InstitutionId { get; private set; }

        public int ManagerCount { get; private set; }
        public int MemberCount { get; private set; }

        private ICollection<SpaceMember> _members = new HashSet<SpaceMember>();
        public IEnumerable<SpaceMember> Members => _members;

        public void AddUserMember(int userId, SpaceMemberCategory category)
        {
            if (_members.OfType<UserSpaceMember>().Any(x => x.UserId == userId))
            {
                throw Errors.MemberAlreadyPresent.AsException();
            }
            _members.Add(new UserSpaceMember(this, userId, category));
            UpdateMemberCounts();
        }

        public void AddGroupMember(int groupId, SpaceMemberCategory category)
        {
            if (_members.OfType<GroupSpaceMember>().Any(x => x.GroupId == groupId))
            {
                throw Errors.MemberAlreadyPresent.AsException();
            }
            _members.Add(new GroupSpaceMember(this, groupId, category));
            UpdateMemberCounts();
        }

        public void RemoveMember(int id)
        {
            var member = _members.FirstOrDefault(x => x.Id == id);
            if (member is null)
            {
                throw new ArgumentException("Member not found.", nameof(id));
            }
            _members.Remove(member);
            UpdateMemberCounts();
        }

        public void UpdateMemberCategory(int id, SpaceMemberCategory category)
        {
            var member = _members.FirstOrDefault(x => x.Id == id);
            if (member is null)
            {
                throw new ArgumentException("Member not found.", nameof(id));
            }
            member.Category = category;
            UpdateMemberCounts();
        }

        private void UpdateMemberCounts()
        {
            ManagerCount = _members.Count(x => x is UserSpaceMember && x.Category == SpaceMemberCategory.Manager);
            if (ManagerCount == 0)
            {
                throw Errors.LastManager.AsException();
            }
        }

        internal class Configuration : IEntityTypeConfiguration<Space>
        {
            public void Configure(EntityTypeBuilder<Space> builder)
            {
                builder.UseXminAsConcurrencyToken();
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

    public static class SpaceLoadedForEditExtensions
    {
        public static IQueryable<Space> LoadedForEdit(this IQueryable<Space> queryable)
        {
            return queryable.Include(x => x.Members);
        }
    }
}