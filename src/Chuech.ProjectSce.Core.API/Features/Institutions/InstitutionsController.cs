using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Institutions.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Chuech.ProjectSce.Core.API.Features.Institutions;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/institutions")]
[ApiController]
[Authorize]
public class InstitutionsController : ControllerBase
{
    private readonly IMediator _mediator;
        
    public InstitutionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("hello-world")]
    public IActionResult HelloWorld()
    {
        return Ok(new {Message = $"Hello world!! You're authenticated as {User.Identity!.Name}."});
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InstitutionApiModel>>> GetAll()
    {
        var institutions = await _mediator.Send(new GetAccessibleInstitutions.Query());
        return Ok(institutions);
    }
        
    [HttpGet("{institutionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstitutionApiModel>> Get(int institutionId)
    {
        var institution = await _mediator.Send(new GetAccessibleInstitutionById.Query(institutionId));
        return institution == null ? NotFound() : Ok(institution);
    }
        
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InstitutionApiModel>> Create([FromBody] CreateInstitution.Command command)
    {
        return await _mediator.Send(command);
    }
}