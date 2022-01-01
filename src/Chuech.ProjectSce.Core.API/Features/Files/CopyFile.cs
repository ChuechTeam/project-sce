namespace Chuech.ProjectSce.Core.API.Features.Files;

public record CopyFile(string SourceBucket,
    string SourceObjectName,
    string TargetBucket,
    string? TargetObjectName = null)
{
    public record Success;
}