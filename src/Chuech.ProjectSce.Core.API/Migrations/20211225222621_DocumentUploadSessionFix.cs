using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class DocumentUploadSessionFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "upload_url",
                table: "document_upload_sessions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "upload_url",
                table: "document_upload_sessions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
