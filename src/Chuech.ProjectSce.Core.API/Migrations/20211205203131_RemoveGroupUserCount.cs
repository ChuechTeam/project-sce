using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class RemoveGroupUserCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_count",
                table: "groups");

            migrationBuilder.AddColumn<Instant>(
                name: "creation_date",
                table: "institution_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "last_edit_date",
                table: "institution_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "creation_date",
                table: "institution_members");

            migrationBuilder.DropColumn(
                name: "last_edit_date",
                table: "institution_members");

            migrationBuilder.AddColumn<int>(
                name: "user_count",
                table: "groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
