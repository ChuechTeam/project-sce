using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/institutions/{institutionId:int}/members")]
    [ApiController]
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MembersController(IMediator mediator)
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
        public async Task<ActionResult<InstitutionMemberApiModel>> Get(int institutionId, int userId)
        {
            var result = await _mediator.Send(new GetMemberById.Query(institutionId, userId));
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Join([FromBody] JoinAsNewMember.Command command, int institutionId)
        {
            var newCommand = command with {InstitutionId = institutionId};
            var result = await _mediator.Send(newCommand);

            return result switch
            {
                JoinAsNewMember.Result.Success => Ok(),

                JoinAsNewMember.Result.InviteNotFound => NotFound(),

                JoinAsNewMember.Result.AlreadyPresent => Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    type: MemberErrors.AlreadyPresent,
                    title: "You're already a member of this institution."),

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
            await _mediator.Send(new KickMember.Command(institutionId, userId));
            return Ok();
        }
    }
}