using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;

namespace Chuech.ProjectSce.Core.API.Features.Institutions;

public class InstitutionGatewayService
{
    private readonly CoreContext _coreContext;

    public InstitutionGatewayService(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public Task<InstitutionMember> JoinAsync(Institution institution, int userId,
        InstitutionRole institutionRole, EducationalRole educationalRole)
    {
        var member = new InstitutionMember(userId, institution, institutionRole, educationalRole);
        _coreContext.InstitutionMembers.Add(member);

        if (institutionRole is InstitutionRole.Admin)
        {
            institution.NotifyNewAdmin();
        }

        return Task.FromResult(member);
    }

    public async Task QuitAsync(InstitutionMember member)
    {
        _coreContext.InstitutionMembers.Remove(member);
        if (member.InstitutionRole is InstitutionRole.Admin)
        {
            var institution = await _coreContext.Institutions.FindAsync(member.InstitutionId);
            if (institution is null)
            {
                throw new InvalidOperationException("Institution not found while quitting institution. (What?)");
            }

            institution.NotifyAdminLost();
        }
    }
}