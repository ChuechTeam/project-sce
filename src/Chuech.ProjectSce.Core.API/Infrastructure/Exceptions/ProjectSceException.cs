namespace Chuech.ProjectSce.Core.API.Infrastructure.Exceptions;

public class ProjectSceException : Exception
{
    public ProjectSceException(Error error) : base(error.Message)
    {
        Error = error;
    }

    public ProjectSceException(Error error, Exception? innerException) : base(error.Message, innerException)
    {
        Error = error;
    }

    public Error Error { get; }
}
