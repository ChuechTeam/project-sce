using System.Diagnostics;

namespace Chuech.ProjectSce.Core.API.Infrastructure.MediatRPipeline
{
    public class ProfilingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<ProfilingBehavior<TRequest, TResponse>> _logger;

        public ProfilingBehavior(ILogger<ProfilingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                return await next();
            }
            finally
            {
                stopwatch.Stop();
                var milliseconds = stopwatch.Elapsed.TotalMilliseconds;

                _logger.LogInformation("Executed MediatR request {Request:l} in {Time:#0.00}ms",
                    typeof(TRequest).FullName,
                    milliseconds);
            }
        }
    }
}