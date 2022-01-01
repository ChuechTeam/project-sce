﻿// <auto-generated />
using System;
using System.Text.Json;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Resources;
using Chuech.ProjectSce.Core.API.Infrastructure.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chuech.ProjectSce.Core.API.Migrations
{
    [DbContext(typeof(CoreContext))]
    [Migration("20211205203131_RemoveGroupUserCount")]
    partial class RemoveGroupUserCount
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "educational_role", new[] { "teacher", "student", "none" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "institution_role", new[] { "admin", "none" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "resource_type", new[] { "document" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Data.OperationLog", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Kind")
                        .HasColumnType("text")
                        .HasColumnName("kind");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<JsonDocument>("ResultJson")
                        .HasColumnType("jsonb")
                        .HasColumnName("result_json");

                    b.HasKey("Id", "Kind")
                        .HasName("pk_operation_log_id_kind");

                    b.ToTable("operation_logs", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Groups.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<int>("InstitutionId")
                        .HasColumnType("integer")
                        .HasColumnName("institution_id");

                    b.Property<Instant>("LastEditDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_edit_date");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_groups");

                    b.HasIndex("InstitutionId")
                        .HasDatabaseName("ix_groups_institution_id");

                    b.HasIndex("Name", "InstitutionId")
                        .IsUnique()
                        .HasDatabaseName("ix_group_name");

                    b.ToTable("groups", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Groups.GroupUser", b =>
                {
                    b.Property<int>("GroupId")
                        .HasColumnType("integer")
                        .HasColumnName("group_id");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.HasKey("GroupId", "UserId")
                        .HasName("pk_group_user");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_group_users_user_id");

                    b.ToTable("group_users", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AdminCount")
                        .HasColumnType("integer")
                        .HasColumnName("admin_count");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_institutions");

                    b.ToTable("institutions", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Institutions.Members.InstitutionMember", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<int>("InstitutionId")
                        .HasColumnType("integer")
                        .HasColumnName("institution_id");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<EducationalRole>("EducationalRole")
                        .HasColumnType("educational_role")
                        .HasColumnName("educational_role");

                    b.Property<InstitutionRole>("InstitutionRole")
                        .HasColumnType("institution_role")
                        .HasColumnName("institution_role");

                    b.Property<Instant>("LastEditDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_edit_date");

                    b.HasKey("UserId", "InstitutionId")
                        .HasName("pk_institution_members");

                    b.HasIndex("InstitutionId")
                        .HasDatabaseName("ix_institution_members_institution_id");

                    b.ToTable("institution_members", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Invitations.Invitation", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<int>("CreatorId")
                        .HasColumnType("integer")
                        .HasColumnName("creator_id");

                    b.Property<Instant>("ExpirationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expiration_date");

                    b.Property<int>("InstitutionId")
                        .HasColumnType("integer")
                        .HasColumnName("institution_id");

                    b.Property<int>("UsagesLeft")
                        .IsConcurrencyToken()
                        .HasColumnType("integer")
                        .HasColumnName("usages_left");

                    b.HasKey("Id")
                        .HasName("pk_invitations");

                    b.HasIndex("CreatorId")
                        .HasDatabaseName("ix_invitations_creator_id");

                    b.HasIndex("InstitutionId")
                        .HasDatabaseName("ix_invitations_institution_id");

                    b.HasIndex("Id", "ExpirationDate", "UsagesLeft")
                        .HasDatabaseName("ix_invitations_id_expiration_date_usages_left");

                    b.ToTable("invitations", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.DocumentUploadSession", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uuid")
                        .HasColumnName("correlation_id");

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer")
                        .HasColumnName("author_id");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<string>("CurrentState")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("current_state");

                    b.Property<Guid?>("ExpirationTimeoutTokenId")
                        .HasColumnType("uuid")
                        .HasColumnName("expiration_timeout_token_id");

                    b.Property<Error>("Failure")
                        .HasColumnType("jsonb")
                        .HasColumnName("failure");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("file_name");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint")
                        .HasColumnName("file_size");

                    b.Property<bool>("HasSentFileDeletion")
                        .HasColumnType("boolean")
                        .HasColumnName("has_sent_file_deletion");

                    b.Property<int>("InstitutionId")
                        .HasColumnType("integer")
                        .HasColumnName("institution_id");

                    b.Property<string>("ResourceName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("resource_name");

                    b.Property<string>("UploadUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("upload_url");

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("CorrelationId")
                        .HasName("pk_document_upload_sessions");

                    b.ToTable("document_upload_sessions", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Resources.Resource", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer")
                        .HasColumnName("author_id");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<int>("InstitutionId")
                        .HasColumnType("integer")
                        .HasColumnName("institution_id");

                    b.Property<Instant>("LastEditDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_edit_date");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<ResourceType>("Type")
                        .HasColumnType("resource_type")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_resources");

                    b.HasIndex("AuthorId")
                        .HasDatabaseName("ix_resources_author_id");

                    b.HasIndex("InstitutionId")
                        .HasDatabaseName("ix_resources_institution_id");

                    b.ToTable("resources", (string)null);

                    b.HasDiscriminator<ResourceType>("Type");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Members.SpaceMember", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Category")
                        .HasColumnType("integer")
                        .HasColumnName("category");

                    b.Property<Instant>("CreationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_date");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("discriminator");

                    b.Property<int>("SpaceId")
                        .HasColumnType("integer")
                        .HasColumnName("space_id");

                    b.HasKey("Id")
                        .HasName("pk_space_members");

                    b.HasIndex("SpaceId")
                        .HasDatabaseName("ix_space_members_space_id");

                    b.ToTable("space_members", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("SpaceMember");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Space", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("InstitutionId")
                        .HasColumnType("integer")
                        .HasColumnName("institution_id");

                    b.Property<int>("ManagerCount")
                        .HasColumnType("integer")
                        .HasColumnName("manager_count");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("SubjectId")
                        .HasColumnType("integer")
                        .HasColumnName("subject_id");

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id")
                        .HasName("pk_spaces");

                    b.HasIndex("InstitutionId")
                        .HasDatabaseName("ix_spaces_institution_id");

                    b.HasIndex("SubjectId")
                        .HasDatabaseName("ix_spaces_subject_id");

                    b.ToTable("spaces", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Subjects.Subject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Color")
                        .HasColumnType("integer")
                        .HasColumnName("color");

                    b.Property<int>("InstitutionId")
                        .HasColumnType("integer")
                        .HasColumnName("institution_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_subjects");

                    b.HasIndex("InstitutionId")
                        .HasDatabaseName("ix_subjects_institution_id");

                    b.ToTable("subjects", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Users.User", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("display_name");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Resources.Documents.DocumentResource", b =>
                {
                    b.HasBaseType("Chuech.ProjectSce.Core.API.Features.Resources.Resource");

                    b.Property<string>("File")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("file");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint")
                        .HasColumnName("file_size");

                    b.HasDiscriminator().HasValue(ResourceType.Document);
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Members.GroupSpaceMember", b =>
                {
                    b.HasBaseType("Chuech.ProjectSce.Core.API.Features.Spaces.Members.SpaceMember");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer")
                        .HasColumnName("group_id");

                    b.HasIndex("GroupId")
                        .HasDatabaseName("ix_space_members_group_id");

                    b.HasIndex("SpaceId", "GroupId")
                        .IsUnique()
                        .HasDatabaseName("ix_space_members_space_id_group_id");

                    b.HasDiscriminator().HasValue("GroupSpaceMember");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Members.UserSpaceMember", b =>
                {
                    b.HasBaseType("Chuech.ProjectSce.Core.API.Features.Spaces.Members.SpaceMember");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_space_members_user_id");

                    b.HasIndex("SpaceId", "UserId")
                        .IsUnique()
                        .HasDatabaseName("ix_space_members_space_id_user_id");

                    b.HasDiscriminator().HasValue("UserSpaceMember");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Groups.Group", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", "Institution")
                        .WithMany()
                        .HasForeignKey("InstitutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_groups_institutions_institution_id");

                    b.Navigation("Institution");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Groups.GroupUser", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Groups.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_group_users_groups_group_id");

                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_group_users_users_user_id");

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Institutions.Members.InstitutionMember", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", "Institution")
                        .WithMany("Members")
                        .HasForeignKey("InstitutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_institution_members_institutions_institution_id");

                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Users.User", "User")
                        .WithMany("InstitutionMembers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_institution_members_users_user_id");

                    b.Navigation("Institution");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Invitations.Invitation", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Users.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_invitations_users_creator_id");

                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", "Institution")
                        .WithMany()
                        .HasForeignKey("InstitutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_invitations_institutions_institution_id");

                    b.Navigation("Creator");

                    b.Navigation("Institution");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Resources.Resource", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Users.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_resources_users_author_id");

                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", "Institution")
                        .WithMany()
                        .HasForeignKey("InstitutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_resources_institutions_institution_id");

                    b.Navigation("Author");

                    b.Navigation("Institution");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Members.SpaceMember", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Spaces.Space", "Space")
                        .WithMany("Members")
                        .HasForeignKey("SpaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_space_members_spaces_space_id");

                    b.Navigation("Space");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Space", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", "Institution")
                        .WithMany()
                        .HasForeignKey("InstitutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_spaces_institutions_institution_id");

                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Subjects.Subject", "Subject")
                        .WithMany()
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_spaces_subjects_subject_id");

                    b.Navigation("Institution");

                    b.Navigation("Subject");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Subjects.Subject", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", "Institution")
                        .WithMany()
                        .HasForeignKey("InstitutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_subjects_institutions_institution_id");

                    b.Navigation("Institution");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Members.GroupSpaceMember", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Groups.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_space_members_groups_group_id");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Members.UserSpaceMember", b =>
                {
                    b.HasOne("Chuech.ProjectSce.Core.API.Features.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_space_members_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Institutions.Institution", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Spaces.Space", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Chuech.ProjectSce.Core.API.Features.Users.User", b =>
                {
                    b.Navigation("InstitutionMembers");
                });
#pragma warning restore 612, 618
        }
    }
}
