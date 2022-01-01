namespace Chuech.ProjectSce.Core.API.Data;

public class DuplicateOperationLogException : Exception
{
    public DuplicateOperationLogException()
    {
    }

    public DuplicateOperationLogException(string? message) : base(message)
    {
    }

    public DuplicateOperationLogException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
