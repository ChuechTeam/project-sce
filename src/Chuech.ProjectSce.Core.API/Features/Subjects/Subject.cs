using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Subjects;

public class Subject
{
    private Subject()
    {
        Institution = null!;
        Name = null!;
    }

    public Subject(int institutionId, string name, RgbColor color)
    {
        Institution = null!;
        InstitutionId = institutionId;
        Name = name;
        Color = color;
    }

    public int Id { get; set; }
    public Institution Institution { get; set; }
    public int InstitutionId { get; set; }
    public string Name { get; set; }
    public RgbColor Color { get; set; }

    internal class Configuration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.Property(x => x.Color)
                .HasConversion(
                    x => x.Value,
                    x => new RgbColor(x));
        }
    }
}