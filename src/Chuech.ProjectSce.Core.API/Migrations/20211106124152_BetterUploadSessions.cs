using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class BetterUploadSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "resource_id",
                table: "document_upload_sessions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "resource_id",
                table: "document_upload_sessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
