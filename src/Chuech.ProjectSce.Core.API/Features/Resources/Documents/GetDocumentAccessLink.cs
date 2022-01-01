using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Resources.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Minio;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents;

public static class GetDocumentAccessLink
{
    public record Query(Guid ResourceId) : IRequest<GeneratedDocumentAccessLink>;

    public class Handler : IRequestHandler<Query, GeneratedDocumentAccessLink>
    {
        private readonly CoreContext _coreContext;
        private readonly MinioClient _minioClient;
        private readonly AuthBarrier<ResourceAuthorizationService> _authBarrier;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<Handler> _logger;

        public Handler(CoreContext coreContext, MinioClient minioClient,
            AuthBarrier<ResourceAuthorizationService> authBarrier,
            IMemoryCache memoryCache, ILogger<Handler> logger)
        {
            _coreContext = coreContext;
            _minioClient = minioClient;
            _authBarrier = authBarrier;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<GeneratedDocumentAccessLink> Handle(Query request,
            CancellationToken cancellationToken)
        {
            _ = await _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(request.ResourceId, userId)
            );

            var document = await _coreContext.DocumentResources
                .FirstOrDefaultAsync(x => x.Id == request.ResourceId, cancellationToken);

            if (document is null)
            {
                throw new NotFoundException();
            }

            var url = await _memoryCache.GetOrCreateAsync($"doc-access-link:{document.Id}", entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                
                return _minioClient.PresignedGetObjectAsync(
                    DocumentResource.FilesBucket, document.File, (int)Duration.FromHours(1).TotalSeconds);
            });

            return new GeneratedDocumentAccessLink(url);
        }
    }
}

public record GeneratedDocumentAccessLink(string Link); // TODO: More properties (expiration date, etc.)