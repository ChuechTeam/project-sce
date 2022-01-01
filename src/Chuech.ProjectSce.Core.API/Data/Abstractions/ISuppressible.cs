namespace Chuech.ProjectSce.Core.API.Data.Abstractions;

/// <summary>
/// <para>
/// Makes the entity suppressible. A suppressed entity is similar to a soft-deleted one,
/// with the difference that suppressed entities are still acknowledged by the system
/// (i.e. there is no global query filter), considered immutable and hidden
/// to the user in most cases.
/// <b>Any personal information (PII) should be deleted when the entity is marked as suppressed!</b>
/// <br/>
/// Suppressible entities are best used when they are part of many relationships, when
/// database-wise <c>ON DELETE</c> options aren't sufficient (set null or cascade) or
/// when you need event-based deletion — at the cost of stale data. A great example
/// of this can be the <see cref="Features.Users.User"/> entity.
/// </para>
/// <para>
/// The service can still query suppressed entities. This is appropriate when, for instance,
/// reacting to a <c>SomethingDeletedEvent</c> and retrieving what's in the entity just before
/// it was deleted to correctly process entities referencing that thing. 
/// </para>
/// <para>
/// APIs faced to a suppressed entity <i>can</i> act as if that entity does not exist, and
/// must return appropriate errors when this happens. They can also provide a way to
/// differentiate between an entity that has existed but is now deleted, and an entity
/// that has never existed.<br/>
/// API queries returning suppressible entities as part of a relationship
/// (such as resource -> owner) can temporarily return stale data (as if the entity still exists)
/// while event consumers do their job. 
/// </para>
/// </summary>
public interface ISuppressible
{
    /// <summary>
    /// The time at which the entity has been marked as suppressed.
    /// </summary>
    Instant? SuppressionDate { get; }
}

public static class SuppressibleExtensions
{
    public static IQueryable<T> ExcludeSuppressed<T>(this IQueryable<T> queryable) where T : ISuppressible
    {
        return queryable.Where(x => x.SuppressionDate == null);
    }
    
    public static bool IsSuppressed(this ISuppressible suppressible) => suppressible.SuppressionDate is null;
}