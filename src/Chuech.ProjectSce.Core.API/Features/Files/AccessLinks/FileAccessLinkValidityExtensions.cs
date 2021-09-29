using Chuech.ProjectSce.Core.API.Features.Files.Data;

namespace Chuech.ProjectSce.Core.API.Features.Files.AccessLinks
{
    public static class FileAccessLinkValidityExtensions
    {
        public static IQueryable<FileAccessLink> FilterValid(this IQueryable<FileAccessLink> links)
        {
            return FilterValid(links, DateTimeOffset.UtcNow);
        }

        public static IQueryable<FileAccessLink> FilterValid(this IQueryable<FileAccessLink> links,
            DateTimeOffset currentDate)
        {
            return links.Where(x => x.ExpirationDate > currentDate);
        }

        public static IQueryable<FileAccessLink> FilterExpired(this IQueryable<FileAccessLink> links)
        {
            return FilterExpired(links, DateTimeOffset.UtcNow);
        }

        public static IQueryable<FileAccessLink> FilterExpired(this IQueryable<FileAccessLink> links,
            DateTimeOffset currentDate)
        {
            return links.Where(x => x.ExpirationDate < currentDate);
        }
    }
}