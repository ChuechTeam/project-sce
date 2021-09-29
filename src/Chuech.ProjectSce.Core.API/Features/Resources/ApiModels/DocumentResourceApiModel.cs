using Chuech.ProjectSce.Core.API.Data.Resources;

namespace Chuech.ProjectSce.Core.API.Features.Resources.ApiModels
{
    public class DocumentResourceApiModel : ResourceApiModel
    {
        public string File { get; set; } = null!;
        public long FileSize { get; set; }

        public static readonly IMapper<DocumentResource, DocumentResourceApiModel> Mapper = BaseExpression.Merge(
            (DocumentResource x) => new DocumentResourceApiModel
            {
                File = x.File,
                FileSize = x.FileSize
            });

        public static async Task LoadReferencesForMapper(DocumentResource resource, DbContext context,
            CancellationToken cancellationToken = default)
        {
            await context.Entry(resource).Reference(x => x.Author).LoadAsync(cancellationToken);
        }
    }
}