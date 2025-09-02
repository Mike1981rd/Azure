using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppWidgetToStructuralComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsAppWidgetConfig",
                table: "StructuralComponentsSettings",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsAppWidgetConfig",
                table: "StructuralComponentsSettings");
        }
    }
}
