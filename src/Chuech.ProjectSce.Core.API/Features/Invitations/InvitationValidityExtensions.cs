namespace Chuech.ProjectSce.Core.API.Features.Invitations;

public static class InvitationValidityExtensions
{
    public static IQueryable<Invitation> FilterValid(this IQueryable<Invitation> invitations,
        IClock clock)
    {
        return FilterValid(invitations, clock.GetCurrentInstant());
    }

    public static IQueryable<Invitation> FilterValid(this IQueryable<Invitation> invitations,
        Instant currentInstant)
    {
        return invitations
            .Where(x => x.ExpirationDate > currentInstant && x.UsagesLeft > 0);
    }
    
    public static IQueryable<Invitation> FilterInvalid(this IQueryable<Invitation> invitations,
        IClock clock)
    {
        return FilterInvalid(invitations, clock.GetCurrentInstant());
    }
    
    public static IQueryable<Invitation> FilterInvalid(this IQueryable<Invitation> invitations,
        Instant currentInstant)
    {
        return invitations
            .Where(x => x.ExpirationDate < currentInstant || x.UsagesLeft <= 0);
    }
}