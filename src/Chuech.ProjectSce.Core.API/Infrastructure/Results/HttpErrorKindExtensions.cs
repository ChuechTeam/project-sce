using System.Net;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Results;

public static class HttpErrorKindExtensions
{
    public static HttpStatusCode GetStatusCode(this ErrorKind errorKind)
    {
        return errorKind switch
        {
            ErrorKind.General => HttpStatusCode.UnprocessableEntity,
            // NOTE: This is really dumb. Why is 401 Unauthorized referring to AUTHENTICATION???
            // Let's fix this madness :D
            ErrorKind.AuthenticationFailure => HttpStatusCode.Unauthorized,
            ErrorKind.AuthorizationFailure => HttpStatusCode.Forbidden,
            ErrorKind.NotFound => HttpStatusCode.NotFound,
            _ => throw new ArgumentException($"Unknown error kind: {errorKind}", nameof(errorKind))
        };
    }
}