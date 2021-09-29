using System.Net;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Exceptions;

public class NotFoundException : ProjectSceException
{
    public NotFoundException(string? message = null) : base(new Error(message, Kind: ErrorKind.NotFound))
    {
    }
}
