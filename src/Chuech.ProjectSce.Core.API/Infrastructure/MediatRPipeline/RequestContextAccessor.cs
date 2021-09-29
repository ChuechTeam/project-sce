namespace Chuech.ProjectSce.Core.API.Infrastructure.MediatRPipeline
{
    public class RequestContextAccessor
    {
        private readonly Dictionary<object, object> _requestToContext = new();
        private readonly IServiceProvider _serviceProvider;

        public RequestContextAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public RequestContext<T> Get<T>(T request) where T : notnull
        {
            if (_requestToContext.TryGetValue(request, out var data))
            {
                return (RequestContext<T>) data;
            }

            var requestContext = new RequestContext<T>(_serviceProvider, request);
            _requestToContext[request] = requestContext;
            return requestContext;
        }
    }
}