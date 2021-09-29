using MassTransit;

namespace Chuech.ProjectSce.Core.API.Infrastructure.DurableCommands;

/// <summary>
/// Handles durable commands that get sent through using MassTransit's Request/Response.
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public abstract class DurableCommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, TResponse>,
      IConsumer<ProcessDurableCommand<TCommand>>
    where TCommand : class, IRequest<TResponse>
{
    protected IBus Bus { get; }
    private readonly IRequestClient<ProcessDurableCommand<TCommand>> _requestClient;

    public DurableCommandHandler(IBus bus, IRequestClient<ProcessDurableCommand<TCommand>> requestClient)
    {
        Bus = bus;
        _requestClient = requestClient;
    }

    public async Task Consume(ConsumeContext<ProcessDurableCommand<TCommand>> context)
    {
        var requestId = context.RequestId ?? throw new InvalidOperationException("No request id found for durable command.");
        try
        {
            var result = await HandleIdempotently(context.Message.Command, requestId);
            await context.RespondAsync(new DurableCommandResult<TResponse>(result));
        }
        catch (ProjectSceException e)
        {
            await context.RespondAsync(new DurableCommandFailure(e.Error));
        }
    }

    public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var command = new ProcessDurableCommand<TCommand>(request);

        var result = await _requestClient.GetResponse<DurableCommandResult<TResponse>, DurableCommandFailure>(command);
        if (result.Is(out Response<DurableCommandResult<TResponse>> response))
        {
            return response.Message.Result;
        }
        else if (result.Is(out Response<DurableCommandFailure> failure))
        {
            throw failure.Message.Error.AsException();
        }

        throw new InvalidOperationException("Unknown result type.");
    }

    protected abstract Task<TResponse> HandleIdempotently(TCommand command, Guid requestId);
}
