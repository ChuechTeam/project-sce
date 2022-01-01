namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members.Commands;

public record RemoveInstitutionMember(int InstitutionId, int UserId)
    : IRespondWith<RemoveInstitutionMember.Success, RemoveInstitutionMember.Failure>
{
    public record Success;

    public record Failure(Error Error) : ProjectSceFailure(Error);
}