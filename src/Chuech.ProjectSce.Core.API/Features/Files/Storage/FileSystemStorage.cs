using Microsoft.Extensions.Options;

namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public class FileSystemStorage : IFileStorage
    {
        private readonly ILogger<FileSystemStorage> _logger;
        private readonly FilesOptions _options;
        private readonly FileChecker _fileChecker;

        public FileSystemStorage(IOptionsSnapshot<FilesOptions> options,
            ILogger<FileSystemStorage> logger,
            FileChecker fileChecker)
        {
            _logger = logger;
            _fileChecker = fileChecker;
            try
            {
                _options = options.Value;
            }
            catch (OptionsValidationException ex)
            {
                foreach (var failure in ex.Failures)
                {
                    _logger.LogError("FileStorage options failure: {Failure}", failure);
                }

                throw;
            }
        }

        public ValueTask<StoredFileInfo?> GetFileAsync(FileIdentifier identifier) 
            => ValueTask.FromResult(GetRawFile(identifier)?.storedInfo);

        public Task<StoredFileWithContents?> GetFileWithContentsAsync(FileIdentifier identifier)
        {
            if (GetRawFile(identifier) is not var (storedInfo, fileInfo))
            {
                return Task.FromResult<StoredFileWithContents?>(null);
            }

            var result = new StoredFileWithContents(fileInfo.OpenRead(), storedInfo);
            return Task.FromResult<StoredFileWithContents?>(result);
        }

        private (StoredFileInfo storedInfo, FileInfo fsInfo)? GetRawFile(FileIdentifier identifier)
        {
            var path = GetPath(identifier);

            if (!File.Exists(path))
            {
                return null;
            }

            var fsInfo = new FileInfo(path);
            var storedInfo = new StoredFileInfo(identifier, fsInfo.Length, fsInfo.Extension);
                
            return (storedInfo, fsInfo);
        }

        public async Task<FileIdentifier> StoreFileAsync(FileCategory category, IFormFile file)
        {
            _fileChecker.Check(file, out var fileExtension);

            var identifier = new FileIdentifier(category, Guid.NewGuid().ToString("N") + fileExtension);
            var filePath = GetPath(identifier);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
            await file.CopyToAsync(stream);

            _logger.LogInformation("Successfully stored file as {Identifier}", identifier);

            return identifier;
        }

        public Task<FileDeletionResult> DeleteFileAsync(FileIdentifier identifier)
        {
            var path = GetPath(identifier);

            if (File.Exists(path))
            {
                File.Delete(path);
                return Task.FromResult(FileDeletionResult.Success);
            }

            return Task.FromResult(FileDeletionResult.FileNotFound);
        }

        public ValueTask<bool> HasFileAsync(FileIdentifier identifier)
        {
            var path = GetPath(identifier);
            return ValueTask.FromResult(File.Exists(path));
        }

        private string GetPath(FileIdentifier identifier)
        {
            var paddedFileName = identifier.FileName.PadLeft(6);
            var depth1 = paddedFileName[..2];
            var depth2 = paddedFileName[2..4];
            var depth3 = paddedFileName[5..];

            return Path.Combine(_options.Location, identifier.Category.Name, depth1, depth2, depth3);
        }
    }
}