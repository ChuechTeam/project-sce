namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions;

public record CancelDocumentUploadSession(Guid SessionId)
    : IRespondWith<CancelDocumentUploadSession.Success, CancelDocumentUploadSession.Failure>
{
    public record Success;

    public record Failure(Error Error) : ProjectSceFailure(Error);
}