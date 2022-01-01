using System.Text.Json.Serialization;
using Chuech.ProjectSce.Core.API.Features.Resources.Documents;
using Chuech.ProjectSce.Core.API.Features.Users.ApiModels;
using NotSoAutoMapper.Polymorphism;

namespace Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;

[JsonConverter(typeof(ForceDerivedPropertiesConverter<ResourceApiModel>))]
public class ResourceApiModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public UserApiModel Author { get; set; } = null!;
    public ResourceType Type { get; set; }

    public Instant CreationDate { get; set; }
    public Instant LastEditDate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<int>? PublicationLocations { get; set; }

    protected static readonly Mapper<Resource, ResourceApiModel> BaseExpression = new(x => new ResourceApiModel
    {
        Id = x.Id,
        Name = x.Name,
        Author = x.Author.MapWith(UserApiModel.Mapper),
        Type = x.Type,
        CreationDate = x.CreationDate,
        LastEditDate = x.LastEditDate,
        // TODO: Make this optional
        PublicationLocations = x.PublishedSpaces.Select(s => s.Id).ToList()
    });

    public static readonly PolymorphicMapper<Resource, ResourceApiModel> PolymorphicMapper =
        new PolymorphicMapperBuilder<Resource, ResourceApiModel>()
            .MapType(() => DocumentResourceApiModel.Mapper)
            .Build();
}