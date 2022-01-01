using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Groups.Commands;

public class SuppressGroupConsumer : IConsumer<SuppressGroup>
{
    private readonly CoreContext _coreContext;
    private readonly IClock _clock;
    private readonly ILogger<SuppressGroupConsumer> _logger;

    public SuppressGroupConsumer(CoreContext coreContext, IClock clock, ILogger<SuppressGroupConsumer> logger)
    {
        _coreContext = coreContext;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SuppressGroup> context)
    {
        var group = await _coreContext.Groups.FindAsync(context.Message.GroupId);
        if (group is null)
        {
            await context.RespondIfNeededAsync(new SuppressGroup.NotFound());
            _logger.LogInformation("Failed to suppress group {Id}: Not found", context.Message.GroupId);
            return;
        }

        if (!group.IsSuppressed())
        {
            group.MarkAsSuppressed(_clock);
            await _coreContext.SaveChangesAsync();
        }

        await context.Publish(new GroupSuppressed(group.Id));
        await context.RespondIfNeededAsync(new SuppressGroup.Success());
        _logger.LogInformation("Group {GroupId} suppressed", group.Id);
    }
}