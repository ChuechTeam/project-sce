using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class NodaTimeAndGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operation_logs",
                table: "operation_logs");

            migrationBuilder.DropIndex(
                name: "ix_operation_log_id_kind",
                table: "operation_logs");

            migrationBuilder.DropColumn(
                name: "member_count",
                table: "spaces");

            migrationBuilder.DropColumn(
                name: "completion_date",
                table: "operation_logs");

            migrationBuilder.RenameIndex(
                name: "ix_groups_name_institution_id",
                table: "groups",
                newName: "ix_group_name");

            migrationBuilder.AddColumn<Instant>(
                name: "creation_date",
                table: "space_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AlterColumn<Instant>(
                name: "last_edit_date",
                table: "resources",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "creation_date",
                table: "resources",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<Instant>(
                name: "creation_date",
                table: "operation_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AlterColumn<Instant>(
                name: "expiration_date",
                table: "invitations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "creation_date",
                table: "invitations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "creation_date",
                table: "institutions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "last_edit_date",
                table: "groups",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "creation_date",
                table: "groups",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operation_log_id_kind",
                table: "operation_logs",
                columns: new[] { "id", "kind" });

            migrationBuilder.CreateTable(
                name: "group_users",
                columns: table => new
                {
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    creation_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_user", x => new { x.group_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_group_users_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_group_users_user_id",
                table: "group_users",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_operation_log_id_kind",
                table: "operation_logs");

            migrationBuilder.DropColumn(
                name: "creation_date",
                table: "space_members");

            migrationBuilder.DropColumn(
                name: "creation_date",
                table: "operation_logs");

            migrationBuilder.RenameIndex(
                name: "ix_group_name",
                table: "groups",
                newName: "ix_groups_name_institution_id");

            migrationBuilder.AddColumn<int>(
                name: "member_count",
                table: "spaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_edit_date",
                table: "resources",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creation_date",
                table: "resources",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "completion_date",
                table: "operation_logs",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "NOW() at time zone 'utc'");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expiration_date",
                table: "invitations",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creation_date",
                table: "invitations",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creation_date",
                table: "institutions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_edit_date",
                table: "groups",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creation_date",
                table: "groups",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "pk_operation_logs",
                table: "operation_logs",
                column: "id");

            migrationBuilder.CreateTable(
                name: "group_user",
                columns: table => new
                {
                    groups_id = table.Column<int>(type: "integer", nullable: false),
                    users_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_user", x => new { x.groups_id, x.users_id });
                    table.ForeignKey(
                        name: "fk_group_user_groups_groups_id",
                        column: x => x.groups_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_user_users_users_id",
                        column: x => x.users_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_operation_log_id_kind",
                table: "operation_logs",
                columns: new[] { "id", "kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_user_users_id",
                table: "group_user",
                column: "users_id");
        }
    }
}
