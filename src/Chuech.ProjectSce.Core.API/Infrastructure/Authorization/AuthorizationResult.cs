using System.Diagnostics.CodeAnalysis;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Authorization;

public abstract record AuthorizationResult(bool IsSuccess)
{
    public void ThrowIfFailed()
    {
        var error = Error;
        if (error is not null)
        {
            throw error.AsException();
        }
    }

    public bool Failed([NotNullWhen(true)] out Error? error)
    {
        error = Error;
        return error is not null;
    }

    public abstract Error? Error { get; }

    public static SuccessAuthorizationResult Success => new();
    public static HiddenForbiddenAuthorizationResult HiddenForbidden => new();
    public static AwareForbiddenAuthorizationResult AwareForbidden(Error error) => new(error);
    public static AwareOtherAuthorizationResult AwareOther(Error error) => new(error);
}

public sealed record SuccessAuthorizationResult() : AuthorizationResult(true)
{
    public override Error? Error => null;
}

public sealed record HiddenForbiddenAuthorizationResult() : AuthorizationResult(false)
{
    private static readonly Error s_error = new(Kind: ErrorKind.NotFound);

    public override Error Error => s_error;
}

public sealed record AwareForbiddenAuthorizationResult
    : AuthorizationResult
{
    public AwareForbiddenAuthorizationResult(Error originalError) : base(false)
    {
        Error = originalError with { Kind = ErrorKind.AuthorizationFailure };
    }

    public override Error Error { get; }
}

public sealed record AwareOtherAuthorizationResult(Error Error) : AuthorizationResult(false)
{
    public override Error Error { get; } = Error;
}