namespace Chuech.ProjectSce.Core.API.Features.Files.UserTrackingStorage
{
    public interface IUserFileStorageLimitProvider
    {
        /// <summary>
        /// Gets the file storage limit for the user.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <returns>The file storage limit.</returns>
        long GetLimit(int userId);
    }

    public class UserFileStorageLimitProvider : IUserFileStorageLimitProvider
    {
        public long GetLimit(int userId)
        {
            // TODO: Make this configurable!
            return 1L * 1024 * 1024 * 1024;
        }
    }
}