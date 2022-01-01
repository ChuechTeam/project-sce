using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;

namespace Chuech.ProjectSce.Core.API.Features.Institutions;

public sealed class Institution : IHaveCreationDate
{
    private Institution()
    {
        Name = null!;
    }

    public Institution(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public Instant CreationDate { get; set; }

    public int AdminCount { get; private set; }

    public void NotifyNewAdmin()
    {
        AdminCount++;
    }

    public void NotifyAdminLost()
    {
        if (AdminCount == 1)
        {
            throw InstitutionMember.Errors.CannotQuitAsLastAdmin.AsException();
        }

        AdminCount--;
    }

    public ICollection<InstitutionMember> Members { get; set; } = new HashSet<InstitutionMember>();

    public static class Errors
    {
        public static readonly Error PermissionMissing = new(
            "You do not have the permission to do this action.",
            "institution.permissionMissing");
    }
}