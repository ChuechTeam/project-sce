using Automatonymous;
using Automatonymous.SagaConfigurators;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Files;
using EntityFramework.Exceptions.Common;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using Minio;
using Minio.Exceptions;

// ReSharper disable MemberInitializerValueIgnored (this thing is bugged)

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions;

public class DocumentUploadSessionStateMachine : MassTransitStateMachine<DocumentUploadSession>
{
    public DocumentUploadSessionStateMachine(ILogger<DocumentUploadSessionStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);

        Event(() => SessionStarted,
            x =>
            {
                x.CorrelateById(context => context.Message.SessionId);
                x.InsertOnInitial = true;

                x.SetSagaFactory(context => CreateSession(context.Message));
            });

        Event(() => ClientFileUploadDoneReported,
            x => x.CorrelateById(context => context.Message.SessionId));

        Event(() => SessionCancelled,
            x =>
            {
                x.CorrelateById(context => context.Message.SessionId);
                x.OnMissingInstance(context =>
                {
                    return context.ExecuteAsync(
                        m => m.RespondAsync(
                            new CancelDocumentUploadSession.Failure(new Error(Kind: ErrorKind.NotFound))));
                });
            });

        Request(() => CopyFile, configureRequest: r => r.Timeout = TimeSpan.Zero);

        Schedule(() => ExpirationTimeout, instance => instance.ExpirationTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(30);
            s.Received = r => r.CorrelateById(context => context.Message.SessionId);
        });

        Initially(
            When(SessionStarted)
                .Then(x => logger.LogInformation("Starting upload session {SessionId}", x.Instance.CorrelationId))
                .Activity(x => x.OfType<GenerateUploadUrlActivity>())
                .Schedule(ExpirationTimeout, x => new DocumentUploadSessionTimeoutExpired(x.Instance.CorrelationId))
                .TransitionTo(WaitingForUpload)
                .Respond(_ => new StartDocumentUploadSession.Success()));

        During(WaitingForUpload,
            When(SessionCancelled)
                .Respond(_ => new CancelDocumentUploadSession.Success())
                .TransitionTo(Cancelled));

        During(States.Except(new[] { WaitingForUpload }),
            When(SessionCancelled)
                .Respond(_ =>
                    new CancelDocumentUploadSession.Failure(DocumentUploadSession.Errors.CancellationImpossible)));

        During(WaitingForUpload,
            When(ClientFileUploadDoneReported)
                .Activity(x => x.OfType<ValidateUploadedFileActivity>())
                .If(x => x.HasPayloadType(typeof(ValidateUploadedFileActivity.Valid)),
                    thenBinder => thenBinder
                        .Then(FillFileSize)
                        .Request(CopyFile, CreateCopyFileRequest)
                        .Then(x => logger.LogInformation(
                            "Uploaded file valid for session {SessionId}",
                            x.Instance.CorrelationId))
                        .TransitionTo(Processing))
                .If(x => x.HasPayloadType(typeof(ValidateUploadedFileActivity.Invalid)),
                    thenBinder => thenBinder
                        .Then(PutFileValidationFailure)
                        .Then(x => logger.LogInformation(
                            "Uploaded file invalid for session {SessionId} with message '{Error}'",
                            x.Instance.CorrelationId,
                            x.GetPayload<ValidateUploadedFileActivity.Invalid>().FileValidationFailure.Message))
                        .TransitionTo(Faulted))
                .If(x => x.HasPayloadType(typeof(ValidateUploadedFileActivity.NoFile)),
                    thenBinder => thenBinder
                        .Then(x => logger.LogInformation(
                            "No uploaded file found after client reported it was uploaded for session {SessionId}",
                            x.Instance.CorrelationId))),
            When(ExpirationTimeout.Received)
                .Then(x => logger.LogInformation(
                    "Expiration timeout reached while waiting for upload for session {SessionId}",
                    x.Instance.CorrelationId))
                .TransitionTo(Expired));

        During(Processing,
            When(CopyFile.Completed)
                .Activity(x => x.OfType<CreateDocumentActivity>())
                .Then(x => logger.LogInformation("Upload session {SessionId} complete", x.Instance.CorrelationId))
                .TransitionTo(Done),
            When(CopyFile.Faulted)
                .Then(x => logger.LogInformation("File copy faulted for session {SessionId}", x.Instance.CorrelationId))
                .TransitionTo(Faulted),
            When(CopyFile.TimeoutExpired)
                .TransitionTo(Faulted),
            When(ExpirationTimeout.Received)
                .Then(x => logger.LogInformation(
                    "Expiration timeout reached while copying file for session {SessionId}",
                    x.Instance.CorrelationId))
                .TransitionTo(Faulted));

        During(Faulted,
            When(CopyFile.Completed)
                .Publish(x => new DeleteFile(DocumentResource.FilesBucket, x.Instance.FileName))
                .Then(x => logger.LogInformation("Deleting file {File} in the files bucket for session {SessionId}",
                    x.Instance.FileName, x.Instance.CorrelationId)));

        DuringAny(
            When(ExpirationTimeout.Received)
                .Then(x => logger.LogInformation(
                    "Deleting file {File} in the uploaded files bucket in session {SessionId}",
                    x.Instance.FileName, x.Instance.CorrelationId))
                .Publish(x => new DeleteFile(DocumentResource.UploadedFilesBucket, x.Instance.FileName))
                .Then(x => x.Instance.HasSentFileDeletion = true)
        );
    }

    private static DocumentUploadSession CreateSession(StartDocumentUploadSession startSession) =>
        new()
        {
            CorrelationId = startSession.SessionId,
            CreationDate = SystemClock.Instance.GetCurrentInstant(),
            ResourceName = startSession.Name,
            InstitutionId = startSession.InstitutionId,
            AuthorId = startSession.AuthorId,
            FileName = $"doc_{startSession.SessionId:N}.{startSession.FileExtension}",
            CurrentState = "Initial" // For some reason this is necessary
        };

    private void FillFileSize(BehaviorContext<DocumentUploadSession, ClientDocumentFileUploadDoneReported> x)
    {
        x.Instance.FileSize = x.GetPayload<ValidateUploadedFileActivity.Valid>().FileSize;
    }

    private static void PutFileValidationFailure(
        BehaviorContext<DocumentUploadSession, ClientDocumentFileUploadDoneReported> context)
    {
        context.Instance.Failure = context.GetPayload<ValidateUploadedFileActivity.Invalid>()
            .FileValidationFailure;
    }

    private static CopyFile CreateCopyFileRequest(
        ConsumeEventContext<DocumentUploadSession, ClientDocumentFileUploadDoneReported> x)
        => new(DocumentResource.UploadedFilesBucket, x.Instance.FileName, DocumentResource.FilesBucket);

    public State WaitingForUpload { get; private set; } = null!;
    public State Processing { get; private set; } = null!;
    public State Done { get; private set; } = null!;
    public State Expired { get; private set; } = null!;
    public State Faulted { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<StartDocumentUploadSession> SessionStarted { get; private set; } = null!;

    public Event<ClientDocumentFileUploadDoneReported> ClientFileUploadDoneReported { get; private set; }
        = null!;

    public Event<CancelDocumentUploadSession> SessionCancelled { get; private set; } = null!;

    public Request<DocumentUploadSession, CopyFile, CopyFile.Success> CopyFile { get; private set; } = null!;

    public Schedule<DocumentUploadSession, DocumentUploadSessionTimeoutExpired> ExpirationTimeout { get; private set; }
        = null!;

    private class GenerateUploadUrlActivity : Activity<DocumentUploadSession, StartDocumentUploadSession>
    {
        private readonly MinioClient _minioClient;
        private readonly ILogger<GenerateUploadUrlActivity> _logger;

        public GenerateUploadUrlActivity(MinioClient minioClient, ILogger<GenerateUploadUrlActivity> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<DocumentUploadSession, StartDocumentUploadSession> context,
            Behavior<DocumentUploadSession, StartDocumentUploadSession> next)
        {
            context.Instance.UploadUrl = await _minioClient.PresignedPutObjectAsync(
                DocumentResource.UploadedFilesBucket, context.Instance.FileName, 60);

            _logger.LogInformation("Created upload URL for session {SessionId}", context.Instance.CorrelationId);

            await next.Execute(context);
        }

        public Task Faulted<TException>(
            BehaviorExceptionContext<DocumentUploadSession, StartDocumentUploadSession, TException> context,
            Behavior<DocumentUploadSession, StartDocumentUploadSession> next) where TException : Exception
        {
            return next.Faulted(context);
        }
    }

    private class ValidateUploadedFileActivity
        : Activity<DocumentUploadSession, ClientDocumentFileUploadDoneReported>
    {
        private readonly MinioClient _minioClient;

        public ValidateUploadedFileActivity(MinioClient minioClient)
        {
            _minioClient = minioClient;
        }

        public void Probe(ProbeContext context)
        {
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<DocumentUploadSession, ClientDocumentFileUploadDoneReported> context,
            Behavior<DocumentUploadSession, ClientDocumentFileUploadDoneReported> next)
        {
            try
            {
                var statObject = await _minioClient.StatObjectAsync(DocumentResource.UploadedFilesBucket,
                    context.Instance.FileName);

                if (statObject.Size > 1024L * 1024 * 1024 * 100)
                {
                    var result = new Invalid(new Error("The file is too large.", "files.maxSizeExceeded"));
                    context.AddOrUpdatePayload(() => result, _ => result);
                }
                else // Success!
                {
                    var result = new Valid(statObject.Size);
                    context.AddOrUpdatePayload(() => result, _ => result);
                }
            }
            catch (MinioException)
            {
                var result = new NoFile();
                context.AddOrUpdatePayload(() => result, _ => result);
            }

            await next.Execute(context);
        }

        public Task Faulted<TException>(
            BehaviorExceptionContext<DocumentUploadSession, ClientDocumentFileUploadDoneReported, TException> context,
            Behavior<DocumentUploadSession, ClientDocumentFileUploadDoneReported> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public record Valid(long FileSize);

        public record Invalid(Error FileValidationFailure);

        public record NoFile;
    }

    private class CreateDocumentActivity : Activity<DocumentUploadSession, CopyFile.Success>
    {
        private readonly CoreContext _coreContext;

        public CreateDocumentActivity(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        public void Probe(ProbeContext context)
        {
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<DocumentUploadSession, CopyFile.Success> context,
            Behavior<DocumentUploadSession, CopyFile.Success> next)
        {
            var document = new DocumentResource(
                context.Instance.ResourceName,
                context.Instance.InstitutionId,
                context.Instance.AuthorId,
                context.Instance.FileName,
                context.Instance.FileSize,
                context.Instance.CorrelationId);

            _coreContext.DocumentResources.Add(document);
            try
            {
                await _coreContext.SaveChangesAsync();
            }
            catch (UniqueConstraintException)
            {
                // Duplicate, but it still exists, so it's all good.
                // There's a very small chance that the resource will be created multiple times
                // if it gets deleted, but it's so tiny that we shouldn't really worry anyway.
            }

            await next.Execute(context);
        }

        public Task Faulted<TException>(
            BehaviorExceptionContext<DocumentUploadSession, CopyFile.Success, TException> context,
            Behavior<DocumentUploadSession, CopyFile.Success> next) where TException : Exception
        {
            return next.Faulted(context);
        }
    }

    public class Definition : SagaDefinition<DocumentUploadSession>
    {
        private const int ConcurrencyValue = 100;

        public Definition()
        {
            ConcurrentMessageLimit = ConcurrencyValue;
        }
        
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
            ISagaConfigurator<DocumentUploadSession> sagaConfigurator)
        {
            var partition = sagaConfigurator.CreatePartitioner(ConcurrencyValue);

            sagaConfigurator.Message<StartDocumentUploadSession>(x =>
                x.UsePartitioner(partition, m => m.Message.SessionId));

            sagaConfigurator.Message<ClientDocumentFileUploadDoneReported>(x =>
                x.UsePartitioner(partition, m => m.Message.SessionId));

            sagaConfigurator.Message<CancelDocumentUploadSession>(x =>
                x.UsePartitioner(partition, m => m.Message.SessionId));

            sagaConfigurator.Message<DocumentUploadSessionTimeoutExpired>(x =>
                x.UsePartitioner(partition, m => m.Message.SessionId));
        }
    }

}