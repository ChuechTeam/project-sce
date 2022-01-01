namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public record InstitutionMemberQuit(int InstitutionId, int UserId, Instant OccurredTime);