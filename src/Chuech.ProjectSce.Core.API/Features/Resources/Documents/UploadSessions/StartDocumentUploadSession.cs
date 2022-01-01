namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions;

public record StartDocumentUploadSession(Guid SessionId, string Name, int InstitutionId, int AuthorId,
    string FileExtension) : IRespondWith<StartDocumentUploadSession.Success>
{
    public record Success;
}