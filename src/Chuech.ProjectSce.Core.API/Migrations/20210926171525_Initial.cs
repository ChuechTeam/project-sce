using System;
using System.Text.Json;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Resources;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Chuech.ProjectSce.Core.API.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:educational_role", "teacher,student,none")
                .Annotation("Npgsql:Enum:institution_role", "admin,none")
                .Annotation("Npgsql:Enum:resource_type", "document");

            migrationBuilder.CreateTable(
                name: "institutions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    admin_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institutions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operation_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    result_json = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    completion_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    user_count = table.Column<int>(type: "integer", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_edit_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_groups_institutions_institution_id",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.id);
                    table.ForeignKey(
                        name: "fk_subjects_institutions_institution_id",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "institution_members",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    institution_role = table.Column<InstitutionRole>(type: "institution_role", nullable: false),
                    educational_role = table.Column<EducationalRole>(type: "educational_role", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institution_members", x => new { x.user_id, x.institution_id });
                    table.ForeignKey(
                        name: "fk_institution_members_institutions_institution_id",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_institution_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    usages_left = table.Column<int>(type: "integer", nullable: false),
                    creator_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_invitations_institutions_institution_id",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_invitations_users_creator_id",
                        column: x => x.creator_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_edit_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    type = table.Column<ResourceType>(type: "resource_type", nullable: false),
                    file = table.Column<string>(type: "text", nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_resources_institutions_institution_id",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resources_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "spaces",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    institution_id = table.Column<int>(type: "integer", nullable: false),
                    manager_count = table.Column<int>(type: "integer", nullable: false),
                    member_count = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spaces", x => x.id);
                    table.ForeignKey(
                        name: "fk_spaces_institutions_institution_id",
                        column: x => x.institution_id,
                        principalTable: "institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_spaces_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "space_members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    space_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_space_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_space_members_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_space_members_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_space_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_group_user_users_id",
                table: "group_user",
                column: "users_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_institution_id",
                table: "groups",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_name_institution_id",
                table: "groups",
                columns: new[] { "name", "institution_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_institution_members_institution_id",
                table: "institution_members",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_creator_id",
                table: "invitations",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_id_expiration_date_usages_left",
                table: "invitations",
                columns: new[] { "id", "expiration_date", "usages_left" });

            migrationBuilder.CreateIndex(
                name: "ix_invitations_institution_id",
                table: "invitations",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_log_id_kind",
                table: "operation_logs",
                columns: new[] { "id", "kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resources_author_id",
                table: "resources",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_institution_id",
                table: "resources",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_space_members_group_id",
                table: "space_members",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_space_members_space_id",
                table: "space_members",
                column: "space_id");

            migrationBuilder.CreateIndex(
                name: "ix_space_members_space_id_group_id",
                table: "space_members",
                columns: new[] { "space_id", "group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_space_members_space_id_user_id",
                table: "space_members",
                columns: new[] { "space_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_space_members_user_id",
                table: "space_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_spaces_institution_id",
                table: "spaces",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "ix_spaces_subject_id",
                table: "spaces",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_institution_id",
                table: "subjects",
                column: "institution_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_user");

            migrationBuilder.DropTable(
                name: "institution_members");

            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "operation_logs");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "space_members");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "spaces");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "institutions");
        }
    }
}
