using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWebsiteBuilderModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "WebsitePages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "WebsitePages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "WebsitePages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "WebsitePages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "WebsitePages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "WebsitePages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomCssClass",
                table: "PageSections",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeOverrides",
                table: "PageSections",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PageSections",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PageSections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "WebsitePages");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "WebsitePages");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "WebsitePages");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "WebsitePages");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "WebsitePages");

            migrationBuilder.DropColumn(
                name: "CustomCssClass",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "ThemeOverrides",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PageSections");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "WebsitePages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
