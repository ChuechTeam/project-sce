using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Data.Resources
{
    public class DocumentResource : Resource
    {
        public DocumentResource() : base(ResourceType.Document)
        {
            File = null!;
        }

        public DocumentResource(string name, int institutionId, int authorId, string file, long fileSize)
            : base(name, institutionId, authorId, ResourceType.Document)
        {
            File = file;
            FileSize = fileSize;
        }

        public string File { get; set; }
        public FileIdentifier FileIdentifier => new(FileCategories.DocumentResources, File);
        public long FileSize { get; set; }
    }
}