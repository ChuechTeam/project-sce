using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.ApiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.Endpoint;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/resources/documents/upload-sessions")]
[ApiController]
[Authorize]
public class DocumentUploadSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentUploadSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{uploadSessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DocumentUploadSessionApiModel>> Get(Guid uploadSessionId)
    {
        var result = await _mediator.Send(new GetSessionById.Query(uploadSessionId));
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<DocumentUploadSessionApiModel>> Create([FromBody] CreateSession.Command command)
    {
        var guid = await _mediator.Send(command);
        return await _mediator.Send(new GetSessionById.Query(guid)) 
               ?? throw new InvalidOperationException("Session null after creation.");
    }

    [HttpPost("{uploadSessionId:guid}/operations/report-upload-done")]
    public async Task<IActionResult> ReportUploadDone(Guid uploadSessionId)
    {
        await _mediator.Send(new ReportUploadDone.Command(uploadSessionId));
        return Accepted();
    }
    
    [HttpPost("{uploadSessionId:guid}/operations/cancel")]
    public async Task<IActionResult> Cancel(Guid uploadSessionId)
    {
        var result = await _mediator.Send(new CancelSession.Command(uploadSessionId));
        return !result.Failed(out var error) ? Ok() : error.AsAspResult(this);
    }
}