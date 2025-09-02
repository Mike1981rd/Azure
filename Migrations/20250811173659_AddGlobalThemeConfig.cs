using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebsiteBuilderAPI.Models.ThemeConfig;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalThemeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalThemeConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Appearance = table.Column<AppearanceConfig>(type: "jsonb", nullable: false),
                    Typography = table.Column<TypographyConfig>(type: "jsonb", nullable: false),
                    ColorSchemes = table.Column<ColorSchemesConfig>(type: "jsonb", nullable: false),
                    ProductCards = table.Column<ProductCardsConfig>(type: "jsonb", nullable: false),
                    ProductBadges = table.Column<ProductBadgesConfig>(type: "jsonb", nullable: false),
                    Cart = table.Column<CartConfig>(type: "jsonb", nullable: false),
                    Favicon = table.Column<FaviconConfig>(type: "jsonb", nullable: false),
                    Navigation = table.Column<NavigationConfig>(type: "jsonb", nullable: false),
                    SocialMedia = table.Column<SocialMediaConfig>(type: "jsonb", nullable: false),
                    Swatches = table.Column<SwatchesConfig>(type: "jsonb", nullable: false),
                    ConfigVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "2.0.0"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalThemeConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalThemeConfigs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalThemeConfigs_CompanyId",
                table: "GlobalThemeConfigs",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalThemeConfigs_IsPublished",
                table: "GlobalThemeConfigs",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalThemeConfigs_UpdatedAt",
                table: "GlobalThemeConfigs",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalThemeConfigs");
        }
    }
}
