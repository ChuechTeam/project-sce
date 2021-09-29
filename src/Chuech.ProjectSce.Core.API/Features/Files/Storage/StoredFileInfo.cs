namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public class StoredFileInfo
    {
        public StoredFileInfo(FileIdentifier identifier, long size, string extension)
        {
            Identifier = identifier;
            Size = size;
            Extension = extension;
        }
        
        public FileIdentifier Identifier { get; }
        public string Extension { get; }
        public long Size { get; }
    }
}