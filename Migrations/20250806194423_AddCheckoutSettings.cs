using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckoutSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ContactMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FullNameOption = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompanyNameField = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddressLine2Field = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PhoneNumberField = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequireShippingAddress = table.Column<bool>(type: "boolean", nullable: false),
                    RequireBillingAddress = table.Column<bool>(type: "boolean", nullable: false),
                    AllowGuestCheckout = table.Column<bool>(type: "boolean", nullable: false),
                    CollectMarketingConsent = table.Column<bool>(type: "boolean", nullable: false),
                    ShowTermsAndConditions = table.Column<bool>(type: "boolean", nullable: false),
                    TermsAndConditionsUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckoutSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutSettings_CompanyId",
                table: "CheckoutSettings",
                column: "CompanyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckoutSettings");
        }
    }
}
