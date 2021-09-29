using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Data.Resources
{
    public abstract class Resource : IFullyDatedEntity
    {
        public Resource(ResourceType type)
        {
            Name = null!;
            Author = null!;
            Institution = null!;
            Type = type;
        }

        public Resource(string name, int institutionId, int authorId, ResourceType type)
        {
            Name = name;
            Author = null!;
            AuthorId = authorId;
            Institution = null!;
            InstitutionId = institutionId;
            Type = type;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public Institution Institution { get; set; }
        public int InstitutionId { get; set; }
        
        public User Author { get; set; }
        public int AuthorId { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastEditDate { get; set; }

        public ResourceType Type { get; set; }

        internal class Configuration : IEntityTypeConfiguration<Resource>
        {
            public void Configure(EntityTypeBuilder<Resource> builder)
            {
                builder.HasDiscriminator(x => x.Type)
                    .HasValue<DocumentResource>(ResourceType.Document);
            }
        }
    }
}