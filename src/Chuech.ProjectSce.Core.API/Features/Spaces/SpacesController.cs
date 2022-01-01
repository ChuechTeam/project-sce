using Chuech.ProjectSce.Core.API.Features.Spaces.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Spaces;
[Route("api/")]
[ApiController]
[Authorize]
public class SpacesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpacesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("institutions/{institutionId:int}/spaces")]
    public async Task<ActionResult<IEnumerable<SpaceApiModel>>> GetAll(int institutionId)
    {
        return Ok(await _mediator.Send(new GetSpaces.Query(institutionId)));
    }

    [HttpGet("spaces/{spaceId:int}")]
    public async Task<ActionResult<SpaceApiModel>> Get(int spaceId)
    {
        var result = await _mediator.Send(new GetSpaceById.Query(spaceId));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("institutions/{institutionId:int}/spaces")]
    public async Task<ActionResult<SpaceApiModel>> Create([FromBody] CreateSpace.Command command, int institutionId)
    {
        return await _mediator.Send(command with { InstitutionId = institutionId });
    }
}
