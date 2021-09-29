using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Data
{
    public class InstitutionMember
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

        public User User { get; set; }
        public int UserId { get; set; }

        public Institution Institution { get; set; }
        public int InstitutionId { get; set; }

        public InstitutionRole InstitutionRole { get; set; }
        public EducationalRole EducationalRole { get; set; }

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
    }

    public static class InstitutionMemberDbSetExtensions
    {
        public static ValueTask<InstitutionMember?> FindByPairAsync(this DbSet<InstitutionMember> dbSet, int userId,
            int institutionId, CancellationToken cancellationToken = default)
        {
            return dbSet.FindAsync(new object[] { userId, institutionId }, cancellationToken)!;
        }
    }
}