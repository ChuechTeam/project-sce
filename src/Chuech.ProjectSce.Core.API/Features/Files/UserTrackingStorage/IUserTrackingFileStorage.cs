using Chuech.ProjectSce.Core.API.Features.Files.Data;
using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Features.Files.UserTrackingStorage
{
    public interface IUserTrackingFileStorage
    {
        Task<long> GetTotalUsedBytesAsync(int userId);
        
        Task<TrackedUserFile> StoreFileAsync(int userId, FileCategory category, IFormFile file);
        
        Task<FileDeletionResult> DeleteFileAsync(int userId, FileIdentifier identifier);
    }

    public class UserTrackingFileStorage : IUserTrackingFileStorage
    {
        private readonly IFileStorage _fileStorage;
        private readonly FileStorageContext _fileStorageContext;
        private readonly IUserFileStorageLimitProvider _storageLimitProvider;
        private readonly ILogger<UserTrackingFileStorage> _logger;

        public UserTrackingFileStorage(IFileStorage fileStorage,
            FileStorageContext fileStorageContext,
            ILogger<UserTrackingFileStorage> logger, 
            IUserFileStorageLimitProvider storageLimitProvider)
        {
            _fileStorage = fileStorage;
            _fileStorageContext = fileStorageContext;
            _logger = logger;
            _storageLimitProvider = storageLimitProvider;
        }

        public Task<long> GetTotalUsedBytesAsync(int userId)
        {
            return _fileStorageContext.TrackedUserFiles
                .Where(x => x.UserId == userId)
                .SumAsync(x => x.FileSize);
        }

        public async Task<TrackedUserFile> StoreFileAsync(int userId, FileCategory category, IFormFile file)
        {
            await EnsureLimitNotExceeded(userId, file);

            var fileIdentifier = await _fileStorage.StoreFileAsync(category, file);

            var trackedFile =
                new TrackedUserFile(fileIdentifier.Category, fileIdentifier.FileName, file.Length, userId);
            _fileStorageContext.Add(trackedFile);
            try
            {
                await _fileStorageContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // Is this a little bit stupid?
                _logger.LogError(e, "Failed save database changes for tracking " +
                                 "file {@FileIdentifier}, deleting the file...", fileIdentifier);
                await _fileStorage.DeleteFileAsync(fileIdentifier);
                throw;
            }

            _logger.LogInformation("Successfully stored a tracked user file: {@File}", trackedFile);
            
            return trackedFile;
        }

        public async Task<FileDeletionResult> DeleteFileAsync(int userId, FileIdentifier identifier)
        {
            var trackedFile = await _fileStorageContext.TrackedUserFiles
                .FirstOrDefaultAsync(x => x.UserId == userId && 
                                          x.Category == identifier.Category && 
                                          x.FileName == identifier.FileName);
            if (trackedFile is null)
            {
                return FileDeletionResult.FileNotFound;
            }
            
            _fileStorageContext.TrackedUserFiles.Remove(trackedFile);
            await _fileStorageContext.SaveChangesAsync();
            
            // TODO: Better error handling
            var result = await _fileStorage.DeleteFileAsync(identifier);
            if (result is FileDeletionResult.FileNotFound)
            {
                _logger.LogWarning("Tracked file {@File} exists but not found in the file storage " +
                                   "while deleting it", trackedFile);
            }

            _logger.LogInformation("Successfully deleted a tracked user file: {@File}", trackedFile);
            
            return FileDeletionResult.Success;
        }

        private async Task EnsureLimitNotExceeded(int userId, IFormFile file)
        {
            var totalBytesAfterwards = await GetTotalUsedBytesAsync(userId) + file.Length;
            var limit = _storageLimitProvider.GetLimit(userId);

            if (totalBytesAfterwards > limit)
            {
                throw FileErrors.StorageLimitExceededError(limit).AsException();
            }
        }
    }
}