using System.Net.Mime;
using Chuech.ProjectSce.Core.API.Features.Files.Data;
using Chuech.ProjectSce.Core.API.Features.Files.Storage;
using Chuech.ProjectSce.Core.API.Features.Files.UserTrackingStorage;
using Chuech.ProjectSce.Core.API.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeTypes;

namespace Chuech.ProjectSce.Core.API.Features.Files
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserTrackingFileStorage _userTrackingFileStorage;
        private readonly IAuthenticationService _authenticationService;
        private readonly FileStorageContext _fileStorageContext;

        public FilesController(IMediator mediator,
            IUserTrackingFileStorage userTrackingFileStorage,
            IAuthenticationService authenticationService, FileStorageContext fileStorageContext)
        {
            _mediator = mediator;
            _userTrackingFileStorage = userTrackingFileStorage;
            _authenticationService = authenticationService;
            _fileStorageContext = fileStorageContext;
        }

        [HttpGet("t/{category}/{id:guid}")]
        [AllowAnonymous] // Because we need browsers to open the URL.
        public async Task<IActionResult> GetTemporary(string category, Guid id)
        {
            var result = await _mediator.Send(new GetAccessLinkFileByCategoryAndId.Query(category, id));
            if (result is var (fileStream, info))
            {
                return File(fileStream, MimeTypeMap.GetMimeType(info.Extension));
            }

            return NotFound();
        }

        [HttpGet("{category}/{file}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublic(string category, string file)
        {
            var result = await _mediator.Send(new GetPublicFileByCategoryAndId.Query(category, file));
            if (result is var (stream, info))
            {
                return File(stream, MimeTypeMap.GetMimeType(info.Extension));
            }

            return NotFound();
        }
        
        [HttpPost("experiment-endpoint/public-test/upload")]
        [Authorize]
        public async Task<IActionResult> StoreSomethingUserTracking([FromForm] IFormFile file)
        {
            var userId = _authenticationService.GetUserId();
            
            var result = await _userTrackingFileStorage.StoreFileAsync(
                userId, FileCategories.PublicTestCategory, file);
            
            return Ok(result);
        }
        
        [HttpGet("experiment-endpoint/usage-info")]
        [Authorize]
        public async Task<IActionResult> GetUsageInfo()
        {
            var userId = _authenticationService.GetUserId();

            var result = await _userTrackingFileStorage.GetTotalUsedBytesAsync(userId);
            var dbEntities = await _fileStorageContext.TrackedUserFiles
                .Where(x => x.UserId == userId)
                .ToArrayAsync();
            
            return Ok(new { BytesUsed = result, Files = dbEntities });
        }
    }
}