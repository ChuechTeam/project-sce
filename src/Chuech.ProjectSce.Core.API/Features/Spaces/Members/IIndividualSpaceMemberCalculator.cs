using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;
public interface IIndividualSpaceMemberCalculator
{
    IEnumerable<IndividualSpaceMember> Calculate(IEnumerable<FlattenedSpaceMember> members);
}
public class IndividualSpaceMemberCalculator : IIndividualSpaceMemberCalculator
{
    public IEnumerable<IndividualSpaceMember> Calculate(IEnumerable<FlattenedSpaceMember> members)
    {
        var userInfos = new Dictionary<(int spaceId, int userId), IndividualAttributes>();
        foreach (var member in members)
        {
            foreach (var userId in member.UserIds)
            {
                var key = (member.SpaceId, userId);

                var currentAttributes = userInfos.GetValueOrDefault(key);
                var newAttributes = new IndividualAttributes(member);

                userInfos[key] = currentAttributes is null ? newAttributes : currentAttributes.Merge(newAttributes);
            }
        }
        return userInfos.Select(kv => kv.Value.MakeIndividualMember(kv.Key.spaceId, kv.Key.userId)).ToArray();
    }
    private record IndividualAttributes(SpaceMemberCategory Category)
    {
        public IndividualAttributes(FlattenedSpaceMember flattenedMember) : this(flattenedMember.Category)
        {
        }
        public IndividualAttributes Merge(IndividualAttributes other)
        {
            return this with
            {
                Category = other.Category > Category ? other.Category : Category
            };
        }
        public IndividualSpaceMember MakeIndividualMember(int spaceId, int userId)
        {
            return new IndividualSpaceMember(spaceId, userId, Category);
        }
    }
}
