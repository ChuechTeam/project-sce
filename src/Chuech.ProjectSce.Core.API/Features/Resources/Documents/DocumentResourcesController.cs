using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/")]
[ApiController]
[Authorize]
public class DocumentResourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentResourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("resources/documents/{resourceId:guid}/access-link")]
    public async Task<ActionResult<GeneratedDocumentAccessLink>> GetLink(Guid resourceId)
    {
        return await _mediator.Send(new GetDocumentAccessLink.Query(resourceId));
    }
}
