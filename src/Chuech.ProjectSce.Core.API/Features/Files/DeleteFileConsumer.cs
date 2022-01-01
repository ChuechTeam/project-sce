using MassTransit;
using Minio;

namespace Chuech.ProjectSce.Core.API.Features.Files;

public class DeleteFileConsumer : IConsumer<DeleteFile>
{
    private readonly MinioClient _minioClient;
    private readonly ILogger<DeleteFileConsumer> _logger;

    public DeleteFileConsumer(MinioClient minioClient, ILogger<DeleteFileConsumer> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeleteFile> context)
    {
        var (bucket, objectName) = context.Message;
        await _minioClient.RemoveObjectAsync(bucket, objectName);

        _logger.LogInformation("Deleted file {File} in {Bucket}", objectName, bucket);
        
        await context.RespondIfNeededAsync(new DeleteFile.Success());
    }
}