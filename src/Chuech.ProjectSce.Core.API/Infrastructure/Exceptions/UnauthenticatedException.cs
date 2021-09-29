using System.Net;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Exceptions
{
    public class UnauthenticatedException : ProjectSceException
    {
        public static readonly Error UnauthenticatedError = 
            new("You are not authenticated", "unauthenticated", ErrorKind.AuthenticationFailure);

        public UnauthenticatedException() : base(UnauthenticatedError)
        {
        }
    }
}