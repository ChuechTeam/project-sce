namespace Chuech.ProjectSce.Core.API.Infrastructure.MediatRPipeline;

public class ExceptionReportingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ExceptionReportingBehavior<TRequest, TResponse>> _logger;

    public ExceptionReportingBehavior(ILogger<ExceptionReportingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return next();
        }
        catch (Exception e)
        {
            if (e is ProjectSceException coreException)
            {
                // We don't use the exception argument here, as we don't need to spit up the whole stacktrace
                // for exceptions that are obvious; there's no need to track the code to know that someone isn't
                // authorized to do something, for instance.
                _logger.LogInformation(
                    "MediatR Request {@Request} failed with message \"{Message}\" ({ErrorCode})",
                    request, e.Message, coreException.Error.ErrorCode);
            }
            else
            {
                // Catastrophic failure!!!
                _logger.LogError(e, "Unexpected error while processing MediatR request {@Request}",
                    request);
            }

            throw;
        }
    }
}