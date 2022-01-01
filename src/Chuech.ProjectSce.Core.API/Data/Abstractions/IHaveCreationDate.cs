namespace Chuech.ProjectSce.Core.API.Data.Abstractions;

public interface IHaveCreationDate
{
    Instant CreationDate { get; set; }
}