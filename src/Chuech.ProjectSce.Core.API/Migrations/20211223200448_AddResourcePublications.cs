using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class AddResourcePublications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resource_publications",
                columns: table => new
                {
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    space_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_publications", x => new { x.resource_id, x.space_id });
                    table.ForeignKey(
                        name: "fk_resource_publications_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_publications_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_resource_publications_space_id",
                table: "resource_publications",
                column: "space_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resource_publications");
        }
    }
}
