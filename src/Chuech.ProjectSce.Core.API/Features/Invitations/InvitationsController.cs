using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Invitations
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class InvitationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvitationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("invitations/{invitationId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous] // UX: Let users see invitations even if they aren't registered
        public async Task<ActionResult<PublicInvitationApiModel>> Get(string invitationId)
        {
            var value = await _mediator.Send(new GetPublicInvitationById.Query(invitationId));
            return value is null ? NotFound() : Ok(value);
        }

        [HttpGet("institutions/{institutionId:int}/invitations/{invitationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<DetailedInvitationApiModel>> Get(int institutionId, string invitationId)
        {
            var value = await _mediator.Send(new GetInstitutionInvitationById.Query(institutionId, invitationId));
            return value is null ? NotFound() : Ok(value);
        }

        [HttpGet("institutions/{institutionId:int}/invitations/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<DetailedInvitationApiModel>>> GetAll(int institutionId)
        {
            return Ok(await _mediator.Send(new GetInstitutionInvitations.Query(institutionId)));
        }

        [HttpPost("institutions/{institutionId:int}/invitations/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<DetailedInvitationApiModel>> Create(
            [FromBody] CreateInvitation.Command command, int institutionId)
        {
            return await _mediator.Send(command with {InstitutionId = institutionId});
        }

        [HttpDelete("institutions/{institutionId:int}/invitations/{invitationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int institutionId, string invitationId)
        {
            await _mediator.Send(new DeleteInvitation.Command(institutionId, invitationId));
            return Ok();
        }
    }
}