using Chuech.ProjectSce.Core.API.Features.Files.Storage;

namespace Chuech.ProjectSce.Core.API.Features.Files.AccessLinks
{
    public interface IFileAccessLinkGenerator
    {
        Task<string> GenerateUrl(FileIdentifier identifier, TimeSpan lifetime)
        {
            return GenerateUrl(identifier, DateTimeOffset.UtcNow, lifetime);
        }
        
        Task<string> GenerateUrl(FileIdentifier identifier, DateTimeOffset currentTime, TimeSpan lifetime);
    }
}