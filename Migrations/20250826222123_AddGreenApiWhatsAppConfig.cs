using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGreenApiWhatsAppConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GreenApiWhatsAppConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    InstanceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApiToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WebhookUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EnableWebhook = table.Column<bool>(type: "boolean", nullable: false),
                    AutoAcknowledgeMessages = table.Column<bool>(type: "boolean", nullable: false),
                    PollingIntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BusinessHoursStart = table.Column<TimeSpan>(type: "interval", nullable: true),
                    BusinessHoursEnd = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AutoReplyMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RateLimitSettings = table.Column<string>(type: "jsonb", nullable: true),
                    BlacklistedNumbers = table.Column<string>(type: "jsonb", nullable: true),
                    AdditionalSettings = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastTestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastTestResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GreenApiWhatsAppConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GreenApiWhatsAppConfigs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GreenApiWhatsAppConfigs_CompanyId",
                table: "GreenApiWhatsAppConfigs",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GreenApiWhatsAppConfigs_InstanceId",
                table: "GreenApiWhatsAppConfigs",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_GreenApiWhatsAppConfigs_IsActive",
                table: "GreenApiWhatsAppConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GreenApiWhatsAppConfigs_PhoneNumber",
                table: "GreenApiWhatsAppConfigs",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GreenApiWhatsAppConfigs");
        }
    }
}
