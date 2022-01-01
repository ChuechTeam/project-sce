namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members.Commands;

public record RemoveSpaceMember(int SpaceId, int MemberId, bool IsExceptional)
    : IRespondWith<RemoveSpaceMember.Success, RemoveSpaceMember.Failure>
{
    public record Success;

    public record Failure(Error Error) : ProjectSceFailure(Error);
}