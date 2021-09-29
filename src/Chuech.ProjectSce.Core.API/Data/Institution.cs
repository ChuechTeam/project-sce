using Chuech.ProjectSce.Core.API.Data.Abstractions;

namespace Chuech.ProjectSce.Core.API.Data
{
    public sealed class Institution : IHaveCreationDate
    {
        private Institution()
        {
            Name = null!;
        }

        public Institution(string name)
        {
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public DateTime CreationDate { get; set; }

        public int AdminCount { get; private set; }

        public void NotifyNewAdmin()
        {
            AdminCount++;
        }

        public void NotifyAdminLost()
        {
            if (AdminCount == 1)
            {
                throw new Error("The last admin in the institution cannot quit the institution or change its role.",
                    "institution.lastAdmin").AsException();
            }
            AdminCount--;
        }

        public ICollection<InstitutionMember> Members { get; set; } = new HashSet<InstitutionMember>();
    }
}