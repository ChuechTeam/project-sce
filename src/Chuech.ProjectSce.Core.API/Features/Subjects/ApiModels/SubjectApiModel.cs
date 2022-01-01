using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Subjects.ApiModels;

public class SubjectApiModel
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string Name { get; set; } = null!;
    public RgbColor Color { get; set; }

    public static readonly Mapper<Subject, SubjectApiModel> Mapper = new(x => new SubjectApiModel
    {
        Id = x.Id,
        InstitutionId = x.InstitutionId,
        Name = x.Name,
        Color = x.Color
    });
}