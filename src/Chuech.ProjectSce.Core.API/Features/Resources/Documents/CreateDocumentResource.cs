using System.Text.Json.Serialization;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Resources;
using Chuech.ProjectSce.Core.API.Features.Files.Storage;
using Chuech.ProjectSce.Core.API.Features.Files.UserTrackingStorage;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents
{
    public static class CreateDocumentResource
    {
        [UseInstitutionAuthorization]
        public sealed record Command(IFormFile File, string Name, [property: JsonIgnore] int InstitutionId)
            : IResourceCreationCommand, IRequest<DocumentResourceApiModel>;

        public class Handler : IRequestHandler<Command, DocumentResourceApiModel>
        {
            private readonly IAuthenticationService _authenticationService;
            private readonly CoreContext _coreContext;
            private readonly IUserTrackingFileStorage _userTrackingFileStorage;
            private readonly ILogger<Handler> _logger;

            public Handler(IAuthenticationService authenticationService, CoreContext coreContext,
                IUserTrackingFileStorage userTrackingFileStorage, ILogger<Handler> logger)
            {
                _authenticationService = authenticationService;
                _coreContext = coreContext;
                _userTrackingFileStorage = userTrackingFileStorage;
                _logger = logger;
            }

            public async Task<DocumentResourceApiModel> Handle(Command request,
                CancellationToken cancellationToken)
            {
                var userId = _authenticationService.GetUserId();

                var documentFile = await _userTrackingFileStorage
                    .StoreFileAsync(userId, FileCategories.DocumentResources, request.File);

                var documentResource = new DocumentResource(
                    request.Name,
                    request.InstitutionId,
                    userId,
                    documentFile.FileName,
                    documentFile.FileSize);

                try
                {
                    _coreContext.Add(documentResource);
                    await _coreContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    var identifier = documentResource.FileIdentifier;

                    _logger.LogError(e,
                        "Failed to add a document resource into the database, deleting created file {Identifier}...",
                        identifier);
                    try
                    {
                        await _userTrackingFileStorage.DeleteFileAsync(userId, identifier);
                    }
                    catch (Exception)
                    {
                        _logger.LogError(e,
                            "Failed to delete the file {Identifier} following a document resource creation failure," +
                            " this file will stay in the file storage!!",
                            identifier);
                    }

                    throw;
                }

                await DocumentResourceApiModel.LoadReferencesForMapper(documentResource, _coreContext,
                    cancellationToken);
                return documentResource.MapWith(DocumentResourceApiModel.Mapper);
            }


            public class Validator : ResourceCreationCommandValidator<Command>
            {
            }
        }
    }
}