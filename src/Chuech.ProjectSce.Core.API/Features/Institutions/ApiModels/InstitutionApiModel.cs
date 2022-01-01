namespace Chuech.ProjectSce.Core.API.Features.Institutions.ApiModels;

public class InstitutionApiModel
{
    public static readonly Mapper<Institution, InstitutionApiModel> Mapper = new(x => new InstitutionApiModel
    {
        Id = x.Id,
        Name = x.Name
    });

    public int Id { get; set; }
    public string Name { get; set; } = null!;
}