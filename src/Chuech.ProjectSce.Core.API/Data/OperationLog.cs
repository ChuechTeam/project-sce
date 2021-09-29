using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Chuech.ProjectSce.Core.API.Data;

public class OperationLog
{
    public const string UniqueIndexName = "ix_operation_log_id_kind";

    private OperationLog()
    {
        Kind = null!;
    }

    public OperationLog(Guid id, string kind, object? result = null)
    {
        Id = id;
        Kind = kind;
        CompletionDate = DateTime.UtcNow;
        SetResult(result);
    }

    public Guid Id { get; private set; }
    public string Kind { get; private set; }

    public JsonDocument? ResultJson { get; private set; }

    public T? GetResult<T>() => ResultJson is null ? default : JsonSerializer.Deserialize<T>(ResultJson);
    public void SetResult<T>(T result) => ResultJson = JsonSerializer.SerializeToDocument(result);

    public DateTime CompletionDate { get; private set; }

    internal class Configuration : IEntityTypeConfiguration<OperationLog>
    {
        public void Configure(EntityTypeBuilder<OperationLog> builder)
        {
            builder.HasIndex(x => new { x.Id, x.Kind })
                .IsUnique()
                .HasDatabaseName(UniqueIndexName);

            builder.Property(x => x.CompletionDate)
                .HasDefaultValueSql("NOW() at time zone 'utc'");
        }
    }
}
