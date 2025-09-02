using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterSubscribers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterSubscribers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SourcePage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SourceCampaign = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "es"),
                    AcceptedMarketing = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptedTerms = table.Column<bool>(type: "boolean", nullable: false),
                    OptInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OptOutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnsubscribeReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ConvertedToCustomer = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscribers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterSubscribers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsletterSubscribers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_CompanyId_Email",
                table: "NewsletterSubscribers",
                columns: new[] { "CompanyId", "Email" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_ConvertedToCustomer",
                table: "NewsletterSubscribers",
                column: "ConvertedToCustomer");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_CreatedAt",
                table: "NewsletterSubscribers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_CustomerId",
                table: "NewsletterSubscribers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_DeletedAt",
                table: "NewsletterSubscribers",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_IsActive",
                table: "NewsletterSubscribers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_Language",
                table: "NewsletterSubscribers",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_OptInDate",
                table: "NewsletterSubscribers",
                column: "OptInDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterSubscribers");
        }
    }
}
