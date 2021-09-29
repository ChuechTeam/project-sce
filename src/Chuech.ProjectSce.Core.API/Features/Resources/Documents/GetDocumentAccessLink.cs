using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Files.AccessLinks;
using Chuech.ProjectSce.Core.API.Features.Resources.Authorization;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents
{
    public static class GetDocumentAccessLink
    {
        [UseResourceAuthorization]
        public record Query(int ResourceId) : IRequest<GeneratedDocumentAccessLink>;
        
        public class Handler : IRequestHandler<Query, GeneratedDocumentAccessLink>
        {
            private readonly CoreContext _coreContext;
            private readonly IFileAccessLinkGenerator _fileAccessLinkGenerator;

            public Handler(CoreContext coreContext, IFileAccessLinkGenerator fileAccessLinkGenerator)
            {
                _coreContext = coreContext;
                _fileAccessLinkGenerator = fileAccessLinkGenerator;
            }

            public async Task<GeneratedDocumentAccessLink> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var document =
                    await _coreContext.DocumentResources.FindAsync(new object[] { request.ResourceId }, cancellationToken);
                if (document is null)
                {
                    throw new NotFoundException("Resource not found.");
                }

                var url = await _fileAccessLinkGenerator.GenerateUrl(document.FileIdentifier, TimeSpan.FromHours(2));
                return new GeneratedDocumentAccessLink(url);
            }
        }
    }
    
    public record GeneratedDocumentAccessLink(string Link); // TODO: More properties (expiration date, etc.)
}