namespace Chuech.ProjectSce.Core.API.Features.Files.Data
{
    // TODO: Make a task that cleans all the expired links
    public sealed class FileAccessLink
    {
        public FileAccessLink()
        {
            FileName = null!;
        }

        public FileAccessLink(string fileName, DateTimeOffset expirationDate)
        {
            FileName = fileName;
            ExpirationDate = expirationDate;
        }

        public Guid Id { get; set; }

        public string FileName { get; set; }
        
        public DateTimeOffset ExpirationDate { get; set; }
    }
}