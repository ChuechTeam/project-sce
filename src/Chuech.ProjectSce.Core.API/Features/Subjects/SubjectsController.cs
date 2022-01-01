using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Subjects.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Subjects;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/institutions/{institutionId:int}/subjects")]
[ApiController]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectApiModel>> Create([FromBody] CreateSubject.Command command, int institutionId)
    {
        return await _mediator.Send(command with { InstitutionId = institutionId });
    }
        
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SubjectApiModel>>> GetAll(int institutionId)
    {
        return Ok(await _mediator.Send(new GetSubjects.Query(institutionId)));
    }
        
    [HttpGet("{subjectId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectApiModel>> Get(int institutionId, int subjectId)
    {
        var subject = await _mediator.Send(new GetSubjectById.Query(institutionId, subjectId));
        return subject is null ? NotFound() : subject;
    }
}