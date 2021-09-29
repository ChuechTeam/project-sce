using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Features.Files
{
    public class FileErrors
    {
        public const string InvalidSize = "files.invalidSize";
        public const string InvalidExtension = "files.invalidExtension";
        public const string StorageLimitExceeded = "files.storageLimitExceeded";

        public static Error InvalidSizeError(long size)
            => new($"Invalid file size of {size} bytes.", InvalidSize);

        public static Error InvalidExtensionError(string fileExtension) 
            => new($"Invalid file extension '{fileExtension}'.", InvalidExtension);

        public static Error StorageLimitExceededError(long limit) 
            => new($"Storage limit of {limit} bytes exceeded.", StorageLimitExceeded);
    }
}