using Chuech.ProjectSce.Core.API.Features.Institutions.Members;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

public class InvalidateInstitutionAuthorizationCacheOnChangesConsumer
    : IConsumer<InstitutionMemberUpdated>,
        IConsumer<InstitutionMemberQuit>
{
    private readonly IInstitutionAuthorizationCache _institutionAuthorizationCache;
    private readonly ILogger<InvalidateInstitutionAuthorizationCacheOnChangesConsumer> _logger;

    public InvalidateInstitutionAuthorizationCacheOnChangesConsumer(
        IInstitutionAuthorizationCache institutionAuthorizationCache,
        ILogger<InvalidateInstitutionAuthorizationCacheOnChangesConsumer> logger)
    {
        _institutionAuthorizationCache = institutionAuthorizationCache;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<InstitutionMemberUpdated> context)
    {
        var (institutionId, userId, _) = context.Message;

        _logger.LogDebug(
            "Invalidating institution authorization cache due to a role change for institution {InstitutionId} and user {UserId}",
            institutionId, userId);
        
        return InvalidateAndNotify(context, institutionId, userId);
    }

    public Task Consume(ConsumeContext<InstitutionMemberQuit> context)
    {
        var (institutionId, userId, _) = context.Message;

        _logger.LogDebug(
            "Invalidating institution authorization cache as the user left for institution {InstitutionId} and user {UserId}",
            institutionId, userId);
        
        return InvalidateAndNotify(context, institutionId, userId);
    }

    private async Task InvalidateAndNotify(IPublishEndpoint context, int institutionId, int userId)
    {
        await _institutionAuthorizationCache.InvalidateAsync(institutionId, userId);

        await context.Publish(new InstitutionAuthorizationChanged(institutionId, new[] { userId }));
    }
}