using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Resources;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/")]
[ApiController]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("institutions/{institutionId:int}/resources")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ResourceApiModel>>> GetAll(int institutionId,
        [FromQuery] ResourceType? resourceType, [FromQuery] int limit = 20, [FromQuery] int start = 0)
    {
        return Ok(await _mediator.Send(new GetResources.Query(resourceType, institutionId, start, limit)));
    }
    
    [HttpPut("resources/{resourceId:guid}/operations/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid resourceId, [FromBody] PublishResourceAction.Command command)
    {
        var result = await _mediator.Send(command with { ResourceId = resourceId });
        return !result.Failed(out var error) ? Ok() : error.AsAspResult(this);
    }
}