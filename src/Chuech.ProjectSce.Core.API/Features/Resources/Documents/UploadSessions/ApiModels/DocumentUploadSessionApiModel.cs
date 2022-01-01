using System.Text.Json.Serialization;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.ApiModels;

public class DocumentUploadSessionApiModel
{
    public Guid Id { get; set; }

    public string State { get; set; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UploadUrl { get; set; }

    public Instant CreationDate { get; set; }

    public static readonly Mapper<DocumentUploadSession, DocumentUploadSessionApiModel> Mapper = new(x =>
        new DocumentUploadSessionApiModel
        {
            Id = x.CorrelationId,
            State = x.CurrentState,
            CreationDate = x.CreationDate,
            UploadUrl = x.CurrentState == "WaitingForUpload" ? x.UploadUrl : null
        });
}