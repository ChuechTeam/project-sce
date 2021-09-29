using Microsoft.Extensions.Options;
using MimeTypes;

namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public sealed class FileChecker
    {
        private readonly ILogger<FileChecker> _logger;
        private readonly FilesOptions _options;

        public FileChecker(ILogger<FileChecker> logger, IOptionsSnapshot<FilesOptions> options)
        {
            _logger = logger;
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

        public void Check(IFormFile file, out string allowedFileExtension)
        {
            if (file.Length > _options.MaximumSize)
            {
                _logger.LogInformation("Can't handle file with a size of {Size} bytes", file.Length);
                throw FileErrors.InvalidSizeError(file.Length).AsException();
            }
            
            var fileExtension = Path.GetExtension(file.FileName);
            if (fileExtension == "." || string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = MimeTypeMap.GetExtension(file.ContentType, false);
            }
            if (string.IsNullOrEmpty(fileExtension) || !_options.AllowedExtensions.Contains(fileExtension))
            {
                _logger.LogInformation("Can't handle file extension '{Extension}'", fileExtension);
                throw FileErrors.InvalidExtensionError(fileExtension).AsException();
            }

            allowedFileExtension = fileExtension;
        }
    }
}