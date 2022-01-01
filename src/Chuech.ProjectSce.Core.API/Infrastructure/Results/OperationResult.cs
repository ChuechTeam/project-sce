using System.Diagnostics.CodeAnalysis;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Results;

public readonly struct OperationResult
{
    public static OperationResult Success() => new(null);
    public static OperationResult<T> Success<T>(T value) => new(value);

    public static OperationResult Failure(Error error) => new(error);
    public static OperationResult<T> Failure<T>(Error error) => new(error);

    public Error? Error { get; }

    private OperationResult(Error? error) { Error = error; }

    public bool IsSuccess => Error is null;

    public void ThrowIfFailed()
    {
        if (!IsSuccess)
        {
            throw Error!.AsException();
        }
    }

    public bool Failed([MaybeNullWhen(false)] out Error error)
    {
        if (!IsSuccess)
        {
            error = Error!;
            return true;
        }
        else
        {
            error = null;
            return false;
        }
    }
}

public readonly struct OperationResult<T>
{
    public T? Value { get; }
    public Error? Error { get; }

    public OperationResult(T value)
    {
        Value = value;
        Error = null;
    }

    public OperationResult(Error error)
    {
        Value = default;
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }

    public bool IsSuccess => Error is null;

    public T GetOrThrow()
    {
        if (IsSuccess)
        {
            return Value!;
        }
        throw Error!.AsException();
    }

    public bool Successful([MaybeNullWhen(false)] out T value)
    {
        if (IsSuccess)
        {
            value = Value!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool Successful([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out Error error)
    {
        if (IsSuccess)
        {
            value = Value!;
            error = null;
            return true;
        }
        else
        {
            value = default;
            error = Error!;
            return false;
        }
    }

    public bool Failed([MaybeNullWhen(false)] out Error error)
    {
        if (IsSuccess)
        {
            error = Error!;
            return true;
        }
        else
        {
            error = null;
            return false;
        }
    }

    public OperationResult WithoutResult()
        => Error is null ? OperationResult.Success() : OperationResult.Failure(Error);
}