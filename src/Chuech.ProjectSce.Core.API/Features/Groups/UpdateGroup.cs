using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Polly;
using Polly.Registry;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public static class UpdateGroup
{
    public record Command([property: JsonIgnore] int GroupId, string Name) : IRequest;

    public class Handler : AsyncRequestHandler<Command>
    {
        private readonly CoreContext _coreContext;
        private readonly AuthBarrier<InstitutionAuthorizationService> _authBarrier;
        private readonly ChuechPolicyRegistry _policyRegistry;

        public Handler(CoreContext coreContext,
            AuthBarrier<InstitutionAuthorizationService> authBarrier,
            ChuechPolicyRegistry policyRegistry)
        {
            _coreContext = coreContext;
            _authBarrier = authBarrier;
            _policyRegistry = policyRegistry;
        }

        protected override async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var group = await _coreContext.AvailableGroups
                .FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);
            if (group is null)
            {
                throw new NotFoundException();
            }

            _ = _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(group.InstitutionId, userId, InstitutionPermission.ManageGroups)
            );

            await _policyRegistry.OptimisticConcurrencyPolicy
                .ExecuteAsync(async _ =>
                {
                    group.UpdateName(group.Name);

                    try
                    {
                        await _coreContext.SaveChangesAsync(cancellationToken);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        await _coreContext.Entry(group).ReloadAsync(cancellationToken);
                        throw;
                    }
                }, new Context($"{nameof(UpdateGroup)}:{request.GroupId}"));
        }
    }

    public class Validator : GroupValidator<Command>
    {
        public Validator()
        {
            AddNameRule(x => x.Name);
        }
    }
}