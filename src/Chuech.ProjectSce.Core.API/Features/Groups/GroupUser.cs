using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Users;
using EntityFramework.Exceptions.Common;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public class GroupUser : IHaveCreationDate, ITransformPersistenceExceptions
{
    public const string KeyName = "pk_group_user";
    
    public GroupUser(Group group, int userId)
    {
        Group = group;
        GroupId = group.Id;
        UserId = userId;
    }
    
    public GroupUser(int groupId, int userId)
    {
        GroupId = groupId;
        UserId = userId;
    }

    public Group Group { get; private set; } = null!;
    public int GroupId { get; private set; }

    public User User { get; private set; } = null!;
    public int UserId { get; private set; }
    
    public Instant CreationDate { get; set; }

    public void Rethrow(DbUpdateException exception)
    {
        if (exception is UniqueConstraintException && exception.Message.Contains(KeyName))
        {
            throw Group.Errors.UserAlreadyPresent.AsException();
        }
    }
}