using Chuech.ProjectSce.Core.API.Features.Institutions;

namespace Chuech.ProjectSce.Core.API.Features.Resources;

public interface IResourceRequest : IInstitutionRequest
{
    int ResourceId { get; }
}