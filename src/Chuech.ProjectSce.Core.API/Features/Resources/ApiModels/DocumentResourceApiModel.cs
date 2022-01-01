using Chuech.ProjectSce.Core.API.Features.Resources.Documents;

namespace Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;

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
}