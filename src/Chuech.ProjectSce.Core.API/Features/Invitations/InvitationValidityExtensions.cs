using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Invitations
{
    public static class InvitationValidityExtensions
    {
        public static IQueryable<Invitation> FilterValid(this IQueryable<Invitation> invitations)
        {
            return FilterValid(invitations, DateTimeOffset.UtcNow);
        }

        public static IQueryable<Invitation> FilterValid(this IQueryable<Invitation> invitations,
            DateTimeOffset currentDate)
        {
            return invitations
                .Where(x => x.ExpirationDate > currentDate && x.UsagesLeft > 0);
        }

        public static IQueryable<Invitation> FilterInvalid(this IQueryable<Invitation> invitations)
        {
            return FilterInvalid(invitations, DateTimeOffset.UtcNow);
        }

        public static IQueryable<Invitation> FilterInvalid(this IQueryable<Invitation> invitations,
            DateTimeOffset currentDate)
        {
            return invitations
                .Where(x => x.ExpirationDate < currentDate || x.UsagesLeft <= 0);
        }
    }
}