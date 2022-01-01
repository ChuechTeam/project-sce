namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public record InstitutionMemberUpdated(int InstitutionId, int UserId, Instant OccurredTime);