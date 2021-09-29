namespace Chuech.ProjectSce.Core.API.Infrastructure.Results;

public sealed record Error(
    string? Message = null,
    string? ErrorCode = null,
    ErrorKind Kind = ErrorKind.General,
    object? AdditionalInfo = null)
{
    public ProjectSceException AsException() => new(this);
    public OperationResult<T> AsOperationResult<T>() => new(this);
}
