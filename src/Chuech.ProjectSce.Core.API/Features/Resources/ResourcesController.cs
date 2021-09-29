using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Resources
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/resources")]
    [ApiController]
    [Authorize]
    public class ResourcesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResourcesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ResourceApiModel>>> GetAll([FromQuery] GetResources.Query query)
        {
            return Ok(await _mediator.Send(query));
        }
    }
}