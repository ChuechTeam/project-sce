using System;
using Chuech.ProjectSce.Core.API.Infrastructure.Results;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class ResourceRevolution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "resources",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
*/
            migrationBuilder.CreateTable(
                name: "document_upload_sessions",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "text", nullable: false),
                    creation_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    upload_url = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_name = table.Column<string>(type: "text", nullable: false),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    failure = table.Column<Error>(type: "jsonb", nullable: true),
                    has_sent_file_deletion = table.Column<bool>(type: "boolean", nullable: false),
                    expiration_timeout_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_upload_sessions", x => x.correlation_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_upload_sessions");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "resources",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
