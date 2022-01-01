using Automatonymous;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions;

public class DocumentUploadSession : SagaStateMachineInstance, IHaveCreationDate
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Instant CreationDate { get; set; }

    public string? UploadUrl { get; set; }

    public string FileName { get; set; } = null!;
    public long FileSize { get; set; }
    public string ResourceName { get; set; } = null!;
    public int InstitutionId { get; set; }
    public int AuthorId { get; set; }
    
    public Error? Failure { get; set; }

    public bool HasSentFileDeletion { get; set; }

    public Guid? ExpirationTimeoutTokenId { get; set; }

    public class Map : SagaClassMap<DocumentUploadSession>
    {
        protected override void Configure(EntityTypeBuilder<DocumentUploadSession> entity, ModelBuilder model)
        {
            entity.Property(x => x.Failure).HasColumnType("jsonb");
            entity.UseXminAsConcurrencyToken();
        }
    }

    public static class Errors
    {
        public static readonly Error CancellationImpossible = new(
            "The session cannot be cancelled.",
            "documentUploadSession.cancellationImpossible");
    }
}