using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Identity.Contract.Events;
using EntityFramework.Exceptions.Common;
using MassTransit;
using MassTransit.Definition;

namespace Chuech.ProjectSce.Core.API.Features.Users;

public class AddUserWhenCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly CoreContext _coreContext;
    private readonly ILogger<AddUserWhenCreatedConsumer> _logger;

    public AddUserWhenCreatedConsumer(CoreContext coreContext, ILogger<AddUserWhenCreatedConsumer> logger)
    {
        _coreContext = coreContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var user = new User
        {
            Id = context.Message.UserId,
            DisplayName = context.Message.DisplayName
        };
        _coreContext.Users.Add(user);
        try
        {
            await _coreContext.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            // Idempotency, the user has already been created.
            _logger.LogInformation("UserCreatedEvent received -> The user {@User} already exists, ignoring.", user);
            return;
        }

        _logger.LogInformation("UserCreatedEvent received -> The user {@User} has been registered", user);
    }

    public class Definition : ConsumerDefinition<AddUserWhenCreatedConsumer>
    {
        public Definition()
        {
            Endpoint(x => x.InstanceId = ":core-api");
        }
    }
}