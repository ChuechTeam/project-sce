
namespace Chuech.ProjectSce.Core.API.Features.Groups;

public static class GroupErrors
{
    public static readonly Error MissingUsersError = 
        new("Some users to be added in the group have not been found.","group.missingUsers");

    public static readonly Error NameTakenError =
        new("The name is already in use.", "group.nameAlreadyTaken");
}
