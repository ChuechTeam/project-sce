using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members.Commands;
using Chuech.ProjectSce.Core.API.Features.Users;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public static class UpdateMember
{
    public record Command(
        [property: JsonIgnore] int InstitutionId,
        [property: JsonIgnore] int UserId,
        InstitutionRole? InstitutionRole = null,
        EducationalRole? EducationalRole = null) : IRequest<OperationResult>;

    public class Handler : IRequestHandler<Command, OperationResult>
    {
        private readonly IRequestClient<UpdateInstitutionMember> _updateClient;
        private readonly IAuthenticationService _authenticationService;
        private readonly InstitutionAuthorizationService _institutionAuthorizationService;

        public Handler(IRequestClient<UpdateInstitutionMember> updateClient,
            IAuthenticationService authenticationService,
            InstitutionAuthorizationService institutionAuthorizationService)
        {
            _updateClient = updateClient;
            _authenticationService = authenticationService;
            _institutionAuthorizationService = institutionAuthorizationService;
        }

        public async Task<OperationResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var initiatorId = _authenticationService.GetUserId();

            var authResult = await _institutionAuthorizationService.AuthorizeAsync(request.InstitutionId, initiatorId,
                InstitutionPermission.ManageMembers);
            if (authResult.Failed(out var error))
            {
                return OperationResult.Failure(error);
            }

            Response response =
                await _updateClient.GetResponse(
                    new UpdateInstitutionMember(
                        request.InstitutionId,
                        request.UserId,
                        request.InstitutionRole,
                        request.EducationalRole), cancellationToken);

            return response switch
            {
                (_, UpdateInstitutionMember.Success) => OperationResult.Success(),
                (_, UpdateInstitutionMember.Failure failure) => OperationResult.Failure(failure.Error),
                _ => throw new InvalidOperationException("Unknown response type.")
            };
        }
    }
}