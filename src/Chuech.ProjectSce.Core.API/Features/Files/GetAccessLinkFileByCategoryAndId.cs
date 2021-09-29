using Chuech.ProjectSce.Core.API.Features.Files.AccessLinks;
using Chuech.ProjectSce.Core.API.Features.Files.Data;
using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Features.Files
{
    public static class GetAccessLinkFileByCategoryAndId
    {
        public record Query(string Category, Guid Id)
            : IRequest<StoredFileWithContents?>;

        public class Handler
            : IRequestHandler<Query, StoredFileWithContents?>
        {
            private readonly FileStorageContext _fileStorageContext;
            private readonly IFileStorage _fileStorage;

            public Handler(FileStorageContext fileStorageContext, IFileStorage fileStorage)
            {
                _fileStorageContext = fileStorageContext;
                _fileStorage = fileStorage;
            }

            public async Task<StoredFileWithContents?> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var (category, linkId) = request;
                if (!FileCategories.TryFind(category, out var actualCategory))
                {
                    return null;
                }

                var fileName = await _fileStorageContext.FileAccessLinks
                    .FilterValid()
                    .Where(x => x.Id == linkId)
                    .Select(x => x.FileName)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!FileIdentifier.TryCreate(actualCategory, fileName, out var fileIdentifier))
                {
                    return null;
                }

                return await _fileStorage.GetFileWithContentsAsync(fileIdentifier);
            }
        }
    }
}