using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Chuech.ProjectSce.Core.API.Features.Files.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_access_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    expiration_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_access_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tracked_user_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tracked_user_files", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_file_access_links_id_expiration_date",
                table: "file_access_links",
                columns: new[] { "id", "expiration_date" })
                .Annotation("Npgsql:IndexInclude", new[] { "file_name" });

            migrationBuilder.CreateIndex(
                name: "ix_tracked_user_files_user_id",
                table: "tracked_user_files",
                column: "user_id")
                .Annotation("Npgsql:IndexInclude", new[] { "file_size" });

            migrationBuilder.CreateIndex(
                name: "ix_tracked_user_files_user_id_file_name_category",
                table: "tracked_user_files",
                columns: new[] { "user_id", "file_name", "category" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_access_links");

            migrationBuilder.DropTable(
                name: "tracked_user_files");
        }
    }
}
