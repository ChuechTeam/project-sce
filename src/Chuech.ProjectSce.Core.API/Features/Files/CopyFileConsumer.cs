using MassTransit;
using Minio;

namespace Chuech.ProjectSce.Core.API.Features.Files;

public class CopyFileConsumer : IConsumer<CopyFile>
{
    private readonly MinioClient _minioClient;
    private readonly ILogger<CopyFileConsumer> _logger;

    public CopyFileConsumer(MinioClient minioClient, ILogger<CopyFileConsumer> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CopyFile> context)
    {
        var message = context.Message;
        await _minioClient.CopyObjectAsync(
            message.SourceBucket,
            message.SourceObjectName,
            message.TargetBucket,
            message.TargetObjectName);

        _logger.LogInformation("Copied file {SourceFile} in {SourceBucket} to {TargetFile} in {TargetBucket}",
            message.SourceObjectName, message.SourceBucket, message.TargetObjectName, message.TargetBucket);

        await context.RespondIfNeededAsync(new CopyFile.Success());
    }
}