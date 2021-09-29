using Chuech.ProjectSce.Core.API.Features.Files.Data;
using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Features.Files.AccessLinks
{
    public sealed class LocalFileAccessLinkGenerator : IFileAccessLinkGenerator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        private readonly FileStorageContext _fileStorageContext;

        public LocalFileAccessLinkGenerator(IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator,
            FileStorageContext fileStorageContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _fileStorageContext = fileStorageContext;
        }

        public async Task<string> GenerateUrl(FileIdentifier identifier,
            DateTimeOffset currentTime, TimeSpan lifetime)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                throw new InvalidOperationException("Cannot generate a temporary URL with a null HttpContext.");
            }

            var link = new FileAccessLink(identifier.FileName, currentTime + lifetime);
            _fileStorageContext.FileAccessLinks.Add(link);

            await _fileStorageContext.SaveChangesAsync();
            
            var id = link.Id;
            return _linkGenerator.GetUriByAction(httpContext,
                controller: "Files",
                action: nameof(FilesController.GetTemporary),
                values: new {category = identifier.Category.Name, id});
        }
    }
}