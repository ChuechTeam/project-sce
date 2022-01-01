using Chuech.ProjectSce.Core.API.Features.Groups.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Groups.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Groups;
[Route("api/")]
[ApiController]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GroupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("institutions/{institutionId:int}/groups")]
    public async Task<ActionResult<IEnumerable<GroupApiModel>>> GetAll(int institutionId, [FromQuery] bool includeUsers = false)
    {
        return Ok(await _mediator.Send(new GetGroups.Query(institutionId, includeUsers)));
    }

    [HttpGet("groups/{groupId:int}")]
    public async Task<ActionResult<IEnumerable<GroupApiModel>>> Get(int groupId)
    {
        return Ok(await _mediator.Send(new GetGroupById.Query(groupId)));
    }

    [HttpPost("institutions/{institutionId:int}/groups/")]
    public async Task<ActionResult<GroupApiModel>> Create([FromBody] CreateGroup.Command command, int institutionId)
    {
        return Ok(await _mediator.Send(command with { InstitutionId = institutionId }));
    }

    [HttpPut("groups/{groupId:int}")]
    public async Task<IActionResult> Update([FromBody] UpdateGroup.Command command, int groupId)
    {
        await _mediator.Send(command with { GroupId = groupId });
        return Ok();
    }

    [HttpDelete("groups/{groupId:int}")]
    public async Task<IActionResult> Remove(int groupId)
    {
        var result = await _mediator.Send(new RemoveGroup.Command(groupId));
        return !result.Failed(out var error) ? Ok() : error.AsAspResult(this);
    }

    [HttpPut("groups/{groupId:int}/users/{userId:int}")]
    public async Task<IActionResult> AddUser(int groupId, int userId)
    {
        await _mediator.Send(new AddUser.Command(groupId, userId));
        return Ok();
    }

    [HttpDelete("groups/{groupId:int}/users/{userId:int}")]
    public async Task<IActionResult> RemoveUser(int groupId, int userId)
    {
        var result = await _mediator.Send(new RemoveUser.Command(groupId, userId));
        return !result.Failed(out var error) ? Ok() : error.AsAspResult(this);
    }
}
