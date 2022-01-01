using MassTransit;
using MassTransit.Topology;

namespace Chuech.ProjectSce.Core.API.Infrastructure;

/// <summary>
/// Defines the response type of this message in a request.
/// </summary>
/// <typeparam name="T1">The response type.</typeparam>
/// <seealso cref="RespondWithRequestClientExtensions.GetResponse{T1,TRequest}"/>
[ExcludeFromTopology]
public interface IRespondWith<T1> where T1 : class
{
}

/// <summary>
/// Defines the response types of this message in a request.
/// </summary>
/// <typeparam name="T1">The first response type.</typeparam>
/// <typeparam name="T2">The second response type.</typeparam>
/// <seealso cref="RespondWithRequestClientExtensions.GetResponse{T1,T2,TRequest}"/>
[ExcludeFromTopology]
public interface IRespondWith<T1, T2> where T1 : class where T2 : class
{
}

public static class RespondWithRequestClientExtensions
{
    public static Task<Response<T1>> GetResponse<T1, TRequest>(this IRequestClient<TRequest> client,
        IRespondWith<T1> message,
        CancellationToken cancellationToken = default,
        RequestTimeout timeout = default)
        where T1 : class
        where TRequest : class, IRespondWith<T1>
    {
        return client.GetResponse<T1>((TRequest)message, cancellationToken, timeout);
    }

    public static Task<Response<T1, T2>> GetResponse<T1, T2, TRequest>(this IRequestClient<TRequest> client,
        IRespondWith<T1, T2> message,
        CancellationToken cancellationToken = default,
        RequestTimeout timeout = default)
        where T1 : class
        where T2 : class
        where TRequest : class, IRespondWith<T1, T2>
    {
        return client.GetResponse<T1, T2>((TRequest)message, cancellationToken, timeout);
    }
}