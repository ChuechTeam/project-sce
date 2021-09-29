using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;
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

    [HttpPost("institutions/{institutionId:int}/resources/documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentResourceApiModel>> Create([FromForm] CreateDocumentResource.Command command,
        int institutionId)
    {
        return await _mediator.Send(command with { InstitutionId = institutionId });
    }

    [HttpGet("resources/documents/{resourceId:int}/access-link")]
    public async Task<ActionResult<GeneratedDocumentAccessLink>> GetLink(int resourceId)
    {
        return await _mediator.Send(new GetDocumentAccessLink.Query(resourceId));
    }
}
