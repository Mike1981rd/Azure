using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishedSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublishedSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    PageSlug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SnapshotData = table.Column<string>(type: "jsonb", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsStale = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishedSnapshots_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublishedSnapshots_WebsitePages_PageId",
                        column: x => x.PageId,
                        principalTable: "WebsitePages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublishedSnapshots_Lookup",
                table: "PublishedSnapshots",
                columns: new[] { "CompanyId", "PageSlug", "IsStale" });

            migrationBuilder.CreateIndex(
                name: "IX_PublishedSnapshots_Version",
                table: "PublishedSnapshots",
                columns: new[] { "PageId", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublishedSnapshots");
        }
    }
}
