namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents;

public class DocumentResource : Resource
{
    public const string FilesBucket = "core.docs";
    public const string UploadedFilesBucket = "core.docs.uploads";
    
    public DocumentResource() : base(ResourceType.Document)
    {
        File = null!;
    }

    public DocumentResource(string name, int institutionId, int authorId, string file, long fileSize, Guid id = default)
        : base(name, institutionId, authorId, ResourceType.Document, id)
    {
        File = file;
        FileSize = fileSize;
    }

    public string File { get; set; }
    public long FileSize { get; set; }
}