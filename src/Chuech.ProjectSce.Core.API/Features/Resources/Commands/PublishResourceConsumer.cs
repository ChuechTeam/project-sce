using Chuech.ProjectSce.Core.API.Data;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Commands;

public class PublishResourceConsumer : IConsumer<PublishResource>
{
    private readonly CoreContext _coreContext;
    private readonly ResourcePublicationValidator _resourcePublicationValidator;
    private readonly ILogger<PublishResourceConsumer> _logger;

    public PublishResourceConsumer(CoreContext coreContext,
        ResourcePublicationValidator resourcePublicationValidator, ILogger<PublishResourceConsumer> logger)
    {
        _coreContext = coreContext;
        _resourcePublicationValidator = resourcePublicationValidator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PublishResource> context)
    {
        var (resourceId, spaceIds) = context.Message;

        async Task Handle()
        {
            var resource = await _coreContext.Resources.FindAsync(resourceId);
            if (resource is null)
            {
                throw new Error(Kind: ErrorKind.NotFound).AsException();
            }

            await resource.PublishAsync(spaceIds, _resourcePublicationValidator);
            await _coreContext.SaveChangesAsync();
        }

        Task PublishEvents() => context.Publish(new ResourcePublished(resourceId, spaceIds));

        try
        {
            await Handle();
            await PublishEvents();
            await context.RespondIfNeededAsync(new PublishResource.Success());
            
            _logger.LogInformation("Published resource {ResourceId} to spaces {@SpaceIds}", resourceId, spaceIds);
        }
        catch (ProjectSceException e)
        {
            await context.RespondIfNeededAsync(new PublishResource.Failure(e.Error));
            
            _logger.LogInformation("Failed to publish resource {ResourceId} to spaces {@SpaceIds}: {@Error}",
                resourceId, spaceIds, e.Error);
        }
    }
}