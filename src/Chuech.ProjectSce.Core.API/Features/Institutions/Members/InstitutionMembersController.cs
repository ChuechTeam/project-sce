using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/institutions/{institutionId:int}/members")]
[ApiController]
[Authorize]
public class InstitutionMembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstitutionMembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<InstitutionMemberApiModel>>> GetAll(int institutionId)
    {
        return Ok(await _mediator.Send(new GetMembers.Query(institutionId)));
    }

    [HttpGet("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstitutionMemberApiModel>> Get(int institutionId, int userId,
        [FromQuery] bool includeAuthorizationInfo)
    {
        var result = await _mediator.Send(new GetMemberById.Query(institutionId, userId, includeAuthorizationInfo));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Join([FromBody] JoinAsNewMember.Command command, int institutionId)
    {
        var newCommand = command with { InstitutionId = institutionId };
        var result = await _mediator.Send(newCommand);

        return result switch
        {
            JoinAsNewMember.Result.Success => Ok(),

            JoinAsNewMember.Result.InviteNotFound => NotFound(),

            JoinAsNewMember.Result.AlreadyPresent => InstitutionMember.Errors.AlreadyPresent.AsAspResult(this),

            _ => throw new NotSupportedException($"Unknown {nameof(JoinAsNewMember.Result)} value: {result}")
        };
    }

    [HttpDelete("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int institutionId, int userId)
    {
        var result = await _mediator.Send(new KickMember.Command(institutionId, userId));
        if (result.Failed(out var error))
        {
            return error.AsAspResult(this);
        }

        return Ok();
    }
    
    [HttpPatch("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int institutionId, int userId,
        [FromBody] UpdateMember.Command command)
    {
        var result = await _mediator.Send(command with { InstitutionId = institutionId, UserId = userId });
        return !result.Failed(out var error) ? Ok() : error.AsAspResult(this);
    }
}