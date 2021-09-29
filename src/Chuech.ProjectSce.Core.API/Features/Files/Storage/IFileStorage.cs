namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public interface IFileStorage
    {
        ValueTask<StoredFileInfo?> GetFileAsync(FileIdentifier identifier);

        Task<StoredFileWithContents?> GetFileWithContentsAsync(FileIdentifier identifier);

        Task<FileIdentifier> StoreFileAsync(FileCategory category, IFormFile file);

        Task<FileDeletionResult> DeleteFileAsync(FileIdentifier identifier);

        ValueTask<bool> HasFileAsync(FileIdentifier identifier);
    }
}