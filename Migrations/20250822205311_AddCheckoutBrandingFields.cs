using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutBrandingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutLogoAlignment",
                table: "CheckoutSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutLogoUrl",
                table: "CheckoutSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckoutLogoWidthPx",
                table: "CheckoutSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutPayButtonColor",
                table: "CheckoutSettings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutPayButtonTextColor",
                table: "CheckoutSettings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutLogoAlignment",
                table: "CheckoutSettings");

            migrationBuilder.DropColumn(
                name: "CheckoutLogoUrl",
                table: "CheckoutSettings");

            migrationBuilder.DropColumn(
                name: "CheckoutLogoWidthPx",
                table: "CheckoutSettings");

            migrationBuilder.DropColumn(
                name: "CheckoutPayButtonColor",
                table: "CheckoutSettings");

            migrationBuilder.DropColumn(
                name: "CheckoutPayButtonTextColor",
                table: "CheckoutSettings");
        }
    }
}
