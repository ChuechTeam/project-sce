using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using EntityFramework.Exceptions.Common;

namespace Chuech.ProjectSce.Core.API.Data;

public class OperationLog : IHaveCreationDate, ITransformPersistenceExceptions
{
    public const string KeyName = "pk_operation_log_id_kind";

    private OperationLog()
    {
        Kind = null!;
    }

    public OperationLog(Guid id, string kind, object? result = null)
    {
        Id = id;
        Kind = kind;
        _lastResult = result;
        ResultJson = result is not null ? JsonSerializer.SerializeToDocument(result) : null;
    }

    public Guid Id { get; private set; }
    public string Kind { get; private set; }

    [NotMapped] private object? _lastResult;
    public JsonDocument? ResultJson { get; private set; }

    public T? GetResult<T>() where T : notnull
    {
        if (ResultJson is null)
        {
            return default;
        }

        if (_lastResult is T typedResult)
        {
            return typedResult;
        }

        var result = ResultJson.Deserialize<T>();
        _lastResult = result;
        return result;
    }

    public T GetResultOrThrow<T>() where T : notnull
    {
        return GetResult<T>() ??
               throw new InvalidOperationException(
                   $"Couldn't find a result of type {typeof(T).Name} in the operation log.");
    }

    public Instant CreationDate { get; set; }

    internal class Configuration : IEntityTypeConfiguration<OperationLog>
    {
        public void Configure(EntityTypeBuilder<OperationLog> builder)
        {
            builder.HasKey(x => new { x.Id, x.Kind })
                .HasName(KeyName);
        }
    }

    public void Rethrow(DbUpdateException exception)
    {
        if (exception is UniqueConstraintException && exception.Message.Contains(KeyName))
        {
            throw new DuplicateOperationLogException("The operation has already been completed.", exception);
        }
    }
}