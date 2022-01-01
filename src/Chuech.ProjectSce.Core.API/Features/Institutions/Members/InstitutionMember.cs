using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public class InstitutionMember : IFullyDatedEntity
{
    private InstitutionMember()
    {
        User = null!;
        Institution = null!;
    }

    public InstitutionMember(int userId, Institution institution, InstitutionRole institutionRole,
        EducationalRole educationalRole)
    {
        User = null!;
        Institution = institution;
        UserId = userId;
        InstitutionRole = institutionRole;
        EducationalRole = educationalRole;
    }

    public User User { get; private set; }
    public int UserId { get; private set; }

    public Institution Institution { get; private set; }
    public int InstitutionId { get; private set; }

    public InstitutionRole InstitutionRole { get; private set; }
    public EducationalRole EducationalRole { get; private set; }

    public Instant CreationDate { get; set; }
    public Instant LastEditDate { get; set; }

    public void UpdateInstitutionRole(InstitutionRole newRole, Institution currentInstitution)
    {
        if (currentInstitution.Id != InstitutionId)
        {
            throw new ArgumentException("Given institution is not the member's institution.",
                nameof(currentInstitution));
        }
        
        InstitutionRole = newRole;
        if (newRole is not InstitutionRole.Admin && InstitutionRole is InstitutionRole.Admin)
        {
            currentInstitution.NotifyAdminLost();
        }
    }

    public void UpdateEducationalRole(EducationalRole newRole) => EducationalRole = newRole;

    internal class Configuration : IEntityTypeConfiguration<InstitutionMember>
    {
        public void Configure(EntityTypeBuilder<InstitutionMember> builder)
        {
            builder.HasKey(x => new { x.UserId, x.InstitutionId });

            builder.HasOne(x => x.User)
                .WithMany(x => x.InstitutionMembers)
                .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.Institution)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.InstitutionId);
        }
    }

    public static class Errors
    {
        public static readonly Error AlreadyPresent = new(
            "You're already a member of this institution.",
            "institution.member.alreadyPresent");

        public static readonly Error CannotQuitAsLastAdmin = new(
            "Cannot quit the institution as you are the last administrator present.",
            "institution.member.cannotQuitAsLastAdmin");

        public static Error CannotKickHigherInHierarchy => new(
            "Cannot kick someone that is higher in the hierarchy than you.",
            "institution.member.cannotKickHigherInHierarchy");
    }
}

public static class InstitutionMemberDbSetExtensions
{
    public static ValueTask<InstitutionMember?> FindByPairAsync(this DbSet<InstitutionMember> dbSet, int userId,
        int institutionId, CancellationToken cancellationToken = default)
    {
        return dbSet.FindAsync(new object[] { userId, institutionId }, cancellationToken)!;
    }
}