using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Features.Files
{
    public static class GetPublicFileByCategoryAndId
    {
        public record Query(string Category, string File) : IRequest<StoredFileWithContents?>;

        public class Handler : IRequestHandler<Query, StoredFileWithContents?>
        {
            private readonly IFileStorage _fileStorage;

            public Handler(IFileStorage fileStorage)
            {
                _fileStorage = fileStorage;
            }

            public async Task<StoredFileWithContents?> Handle(Query request,
                CancellationToken cancellationToken)
            {
                if (!FileIdentifier.IsValidFileName(request.File) ||
                    !FileCategories.TryFind(request.Category, out var fileCategory) ||
                    !fileCategory.IsPublic)
                {
                    return null;
                }

                return await _fileStorage.GetFileWithContentsAsync(new FileIdentifier(fileCategory, request.File));
            }
        }
    }
}