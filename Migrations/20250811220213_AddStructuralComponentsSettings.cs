using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuralComponentsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditorHistories_WebsitePages_PageId",
                table: "EditorHistories");

            migrationBuilder.AlterColumn<int>(
                name: "PageId",
                table: "EditorHistories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ChangeType",
                table: "EditorHistories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "EditorHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "EditorHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "EditorHistories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "EditorHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckpoint",
                table: "EditorHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "EditorHistories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "EditorHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StructuralComponentsSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    HeaderConfig = table.Column<string>(type: "jsonb", nullable: false),
                    AnnouncementBarConfig = table.Column<string>(type: "jsonb", nullable: false),
                    FooterConfig = table.Column<string>(type: "jsonb", nullable: false),
                    CartDrawerConfig = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructuralComponentsSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StructuralComponentsSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StructuralComponentsSettings_Users_LastUpdatedBy",
                        column: x => x.LastUpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EditorHistories_CompanyId",
                table: "EditorHistories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorHistories_UserId",
                table: "EditorHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StructuralComponentsSettings_CompanyId",
                table: "StructuralComponentsSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_StructuralComponentsSettings_LastUpdatedBy",
                table: "StructuralComponentsSettings",
                column: "LastUpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_EditorHistories_Companies_CompanyId",
                table: "EditorHistories",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EditorHistories_Users_UserId",
                table: "EditorHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EditorHistories_WebsitePages_PageId",
                table: "EditorHistories",
                column: "PageId",
                principalTable: "WebsitePages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditorHistories_Companies_CompanyId",
                table: "EditorHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_EditorHistories_Users_UserId",
                table: "EditorHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_EditorHistories_WebsitePages_PageId",
                table: "EditorHistories");

            migrationBuilder.DropTable(
                name: "StructuralComponentsSettings");

            migrationBuilder.DropIndex(
                name: "IX_EditorHistories_CompanyId",
                table: "EditorHistories");

            migrationBuilder.DropIndex(
                name: "IX_EditorHistories_UserId",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "ChangeType",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "IsCheckpoint",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "EditorHistories");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "EditorHistories");

            migrationBuilder.AlterColumn<int>(
                name: "PageId",
                table: "EditorHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EditorHistories_WebsitePages_PageId",
                table: "EditorHistories",
                column: "PageId",
                principalTable: "WebsitePages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
