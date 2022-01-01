namespace Chuech.ProjectSce.Core.API.Data.Abstractions;

public interface IHaveLastEditDate
{
    Instant LastEditDate { get; set; }
}