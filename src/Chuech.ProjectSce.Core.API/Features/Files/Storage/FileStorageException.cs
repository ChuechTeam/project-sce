using System.Net;

namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public class FileStorageException : ProjectSceException
    {
        public FileStorageException(Error error) : base(error)
        {
        }

        public FileStorageException(Error error, Exception? innerException) : base(error, innerException)
        {
        }
    }
}