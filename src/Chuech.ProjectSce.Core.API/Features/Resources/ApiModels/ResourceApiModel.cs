using System.Text.Json.Serialization;
using Chuech.ProjectSce.Core.API.Data.Resources;
using Chuech.ProjectSce.Core.API.Features.Users.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Resources.ApiModels
{
    [JsonConverter(typeof(ForceDerivedPropertiesConverter<ResourceApiModel>))]
    public class ResourceApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public UserApiModel Author { get; set; } = null!;
        public ResourceType Type { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastEditDate { get; set; }

        protected static readonly Mapper<Resource, ResourceApiModel> BaseExpression = new(x => new ResourceApiModel
        {
            Id = x.Id,
            Name = x.Name,
            Author = x.Author.MapWith(UserApiModel.Mapper),
            Type = x.Type,
            CreationDate = x.CreationDate,
            LastEditDate = x.LastEditDate
        });

        private static readonly Lazy<Mapper<Resource, ResourceApiModel>> s_polymorphicMapper =
            new(() => new(x =>
                x is DocumentResource
                    ? ((DocumentResource) x).MapWith(DocumentResourceApiModel.Mapper)
                    : null!));

        public static Mapper<Resource, ResourceApiModel> PolymorphicMapper => s_polymorphicMapper.Value;
    }
}