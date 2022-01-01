namespace Chuech.ProjectSce.Core.API.Features.Files;

public record DeleteFile(string Bucket, string ObjectName)
{
    public record Success;
}