using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Groups;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Data;

public sealed class Group : IFullyDatedEntity
{
    private Group()
    {
        Name = null!;
    }

    public Group(int institutionId, string name)
    {
        Name = name;
        InstitutionId = institutionId;
    }

    public int Id { get; set; }
    public string Name { get; set; }

    public Institution Institution { get; set; } = null!;
    public int InstitutionId { get; set; }

    public ICollection<User> Users { get; set; } = new HashSet<User>();
    public int UserCount { get; set; }

    public DateTime CreationDate { get; set; }
    public DateTime LastEditDate { get; set; }

    /// <summary>
    /// Updates the users of this group.
    /// </summary>
    /// <param name="userIds"></param>
    /// <param name="usersQuery"></param>
    /// <returns></returns>
    public async Task UpdateUsersAsync(IEnumerable<int> userIds, AccessibleUsersFromIdsQuery usersQuery)
    {
        var allUserIds = userIds.ToArray();

        var users = allUserIds.Length != 0 ? await usersQuery.Get(allUserIds, InstitutionId) : Array.Empty<User>();

        Users.Clear();
        foreach (var user in users)
        {
            Users.Add(user);
        }
        UserCount = users.Length;
    }

    internal class Configuration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasIndex(x => new { x.Name, x.InstitutionId })
                .IsUnique();
        }
    }
}