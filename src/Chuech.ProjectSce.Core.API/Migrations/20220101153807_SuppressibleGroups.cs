using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class SuppressibleGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "suppression_date",
                table: "groups",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "suppression_date",
                table: "groups");
        }
    }
}
