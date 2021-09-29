using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members
{
    public static class MemberErrors
    {
        public const string CannotQuitAsLastAdmin = "institution.member.cannotQuitAsLastAdmin";
        public const string CannotKickHigherInHierarchy = "institution.member.cannotKickHigherInHierarchy";
        public const string AlreadyPresent = "institution.member.alreadyPresent";

        public static readonly Error CannotQuitAsLastAdminError = new(
            "Cannot quit the institution as you are the last administrator present.",
                CannotQuitAsLastAdmin);

        public static Error CannotKickHigherInHierarchyError(InstitutionMember kicker,
            InstitutionMember userToKick) => new(
                "Cannot kick someone that is higher in the hierarchy than you. " +
                $"(You're {kicker.InstitutionRole}, they are {userToKick.InstitutionRole})",
                CannotKickHigherInHierarchy);
    }
}