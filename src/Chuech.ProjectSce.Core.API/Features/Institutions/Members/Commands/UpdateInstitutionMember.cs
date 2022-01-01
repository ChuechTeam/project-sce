namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members.Commands;

public record UpdateInstitutionMember(int InstitutionId,
        int UserId,
        InstitutionRole? InstitutionRole,
        EducationalRole? EducationalRole)
    : IRespondWith<UpdateInstitutionMember.Success, UpdateInstitutionMember.Failure>
{
    public record Success;

    public record Failure(Error Error) : ProjectSceFailure(Error);
}