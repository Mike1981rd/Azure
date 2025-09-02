using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookAuthFieldsToWhatsAppConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeaderName",
                table: "WhatsAppConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeaderValueTemplate",
                table: "WhatsAppConfigs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWebhookEventAt",
                table: "WhatsAppConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookSecret",
                table: "WhatsAppConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeaderName",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "HeaderValueTemplate",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "LastWebhookEventAt",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookSecret",
                table: "WhatsAppConfigs");
        }
    }
}
