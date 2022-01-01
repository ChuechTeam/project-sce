using Chuech.ProjectSce.Core.API.Features.Groups;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Users;

public class User
{
    public User()
    {
        DisplayName = null!;
    }

    public User(int id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public int Id { get; set; }
    public string DisplayName { get; set; }

    public ICollection<Group> Groups { get; set; } = new HashSet<Group>();
    public ICollection<InstitutionMember> InstitutionMembers { get; set; } = new HashSet<InstitutionMember>();

    internal class Configuration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.Id)
                .ValueGeneratedNever();
        }
    }
}