using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Subjects.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.ApiModels;
public class SpaceApiModel
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string Name { get; set; } = null!;
    public SubjectApiModel Subject { get; set; } = null!;

    public static readonly Mapper<Space, SpaceApiModel> Mapper = new(x => new SpaceApiModel
    {
        Id = x.Id,
        InstitutionId = x.InstitutionId,
        Name = x.Name,
        Subject = x.Subject.MapWith(SubjectApiModel.Mapper)
    });
}
