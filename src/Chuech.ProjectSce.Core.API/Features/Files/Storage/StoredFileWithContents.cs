namespace Chuech.ProjectSce.Core.API.Features.Files.Storage
{
    public readonly struct StoredFileWithContents : IDisposable
    {
        public StoredFileWithContents(Stream stream, StoredFileInfo file)
        {
            Stream = stream;
            File = file;
        }

        public Stream Stream { get; }
        public StoredFileInfo File { get; }

        public void Deconstruct(out Stream stream, out StoredFileInfo file)
        {
            stream = Stream;
            file = File;
        }

        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}