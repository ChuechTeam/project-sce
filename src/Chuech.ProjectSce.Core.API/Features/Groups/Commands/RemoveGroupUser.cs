namespace Chuech.ProjectSce.Core.API.Features.Groups.Commands;

public record RemoveGroupUser(int GroupId, int UserId) : IRespondWith<RemoveGroupUser.Success, RemoveGroupUser.Failure>
{
    public record Success;

    public record Failure(Error Error) : ProjectSceFailure(Error);
}