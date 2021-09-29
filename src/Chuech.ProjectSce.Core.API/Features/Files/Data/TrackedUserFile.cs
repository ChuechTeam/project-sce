using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Features.Files.Data
{
    public sealed class TrackedUserFile
    {
        public TrackedUserFile()
        {
            FileName = null!;
        }
        
        public TrackedUserFile(FileCategory? category, string fileName, long fileSize, int userId)
        {
            Category = category;
            FileName = fileName;
            FileSize = fileSize;
            UserId = userId;
        }

        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        /// <summary>
        /// The category of the file. A null value indicates that the category does not exist anymore.
        /// </summary>
        public FileCategory? Category { get; set; }
        
        public string FileName { get; set; }

        public FileIdentifier? Identifier => Category == null ? null : new FileIdentifier(Category, FileName);
        
        public long FileSize { get; set; }
    }
}