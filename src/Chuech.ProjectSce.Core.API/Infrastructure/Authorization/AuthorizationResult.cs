using Chuech.ProjectSce.Core.API.Infrastructure.Exceptions;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Authorization;

public abstract record AuthorizationResult(bool IsSuccess)
{
    public void ThrowIfUnsuccessful()
    {
        var error = AsError();
        if (error is not null)
        {
            throw error.AsException();
        }
    }
    public abstract Error? AsError();

    public static SuccessAuthorizationResult Success => new();
    public static AwareForbiddenAuthorizationResult AwareFobidden(string Reason, string ErrorCode) => new(Reason, ErrorCode);
    public static HiddenForbiddenAuthorizationResult HiddenForbidden(string? ErrorMessage = null) => new(ErrorMessage);
}

public sealed record SuccessAuthorizationResult() : AuthorizationResult(true)
{
    public override Error? AsError()
    {
        return null;
    }
}

public sealed record AwareForbiddenAuthorizationResult(string Reason, string ErrorCode)
    : AuthorizationResult(false)
{
    public override Error AsError()
    {
        return new Error(Reason, ErrorCode, ErrorKind.AuthorizationFailure);
    }
}

public sealed record HiddenForbiddenAuthorizationResult(string? ErrorMessage = null) : AuthorizationResult(false)
{
    public override Error AsError()
    {
        return new Error(ErrorMessage, Kind: ErrorKind.NotFound);
    }
}
