namespace Chuech.ProjectSce.Core.API.Infrastructure.MediatRPipeline;

public interface IRequestContext<out T> where T : notnull
{
    public T Request { get; }
    public Dictionary<string, object?> Data { get; }
    public IServiceProvider ServiceProvider { get; }
}
    
public sealed class RequestContext<T> where T : notnull
{
    public RequestContext(IServiceProvider serviceProvider, T request)
    {
        ServiceProvider = serviceProvider;
        Request = request;
    }

    public T Request { get; }
    public Dictionary<string, object?> Data { get; } = new();
    public IServiceProvider ServiceProvider { get; }
}