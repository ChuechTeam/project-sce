namespace Chuech.ProjectSce.Core.API.Data.Abstractions;

public interface ITransformPersistenceExceptions
{
    public static void Dispatch(DbUpdateException exception)
    {
        foreach (var entry in exception.Entries)
        {
            if (entry.Entity is ITransformPersistenceExceptions rethrowingEntity)
            {
                rethrowingEntity.Rethrow(exception);
            }
        }
    }

    void Rethrow(DbUpdateException exception);
}