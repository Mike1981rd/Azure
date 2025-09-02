using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCollectionUniqueIndexForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_CompanyId_Handle",
                table: "Collections");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_CompanyId_Handle",
                table: "Collections",
                columns: new[] { "CompanyId", "Handle" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_CompanyId_Handle",
                table: "Collections");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_CompanyId_Handle",
                table: "Collections",
                columns: new[] { "CompanyId", "Handle" },
                unique: true);
        }
    }
}
