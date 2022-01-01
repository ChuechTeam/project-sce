using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

[Route("api/spaces/{spaceId:int}/members")]
[ApiController]
[Authorize]
public class SpaceMembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpaceMembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetMembers.Result>> GetAll(int spaceId)
    {
        return await _mediator.Send(new GetMembers.Query(spaceId));
    }

    [HttpPost("groups")]
    public async Task<IActionResult> AddGroup(AddMember.GroupCommand command, int spaceId)
    {
        await _mediator.Send(command with { SpaceId = spaceId });
        return Ok();
    }

    [HttpPost("users")]
    public async Task<IActionResult> AddUser(AddMember.UserCommand command, int spaceId)
    {
        await _mediator.Send(command with { SpaceId = spaceId });
        return Ok();
    }

    [HttpDelete("groups")]
    public async Task<IActionResult> RemoveGroup(RemoveMember.GroupCommand command, int spaceId)
    {
        var result = await _mediator.Send(command with { SpaceId = spaceId });
        return !result.Failed(out var error) ? Ok() : error.AsAspResult(this);
    }

    [HttpDelete("users")]
    public async Task<IActionResult> RemoveUser(RemoveMember.UserCommand command, int spaceId)
    {
        var result = await _mediator.Send(command with { SpaceId = spaceId });
        return !result.Failed(out var error) ? Ok() : error.AsAspResult(this);
    }
}