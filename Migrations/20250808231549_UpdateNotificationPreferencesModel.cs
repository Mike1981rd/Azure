using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationPreferencesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerNotificationPreferences_CustomerId_NotificationType",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "CustomerNotificationPreferences");

            migrationBuilder.RenameColumn(
                name: "NotificationType",
                table: "CustomerNotificationPreferences",
                newName: "Timezone");

            migrationBuilder.RenameColumn(
                name: "EmailEnabled",
                table: "CustomerNotificationPreferences",
                newName: "SmsPromotions");

            migrationBuilder.RenameColumn(
                name: "BrowserEnabled",
                table: "CustomerNotificationPreferences",
                newName: "SmsOrderUpdates");

            migrationBuilder.RenameColumn(
                name: "AppEnabled",
                table: "CustomerNotificationPreferences",
                newName: "SmsDeliveryAlerts");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomerNotificationPreferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DoNotDisturbEnd",
                table: "CustomerNotificationPreferences",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DoNotDisturbStart",
                table: "CustomerNotificationPreferences",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailNewsletter",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailOrderUpdates",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailPriceAlerts",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailProductReviews",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailPromotions",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PushEnabled",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PushSound",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PushVibration",
                table: "CustomerNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomerNotificationPreferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationPreferences_CustomerId",
                table: "CustomerNotificationPreferences",
                column: "CustomerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerNotificationPreferences_CustomerId",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "DoNotDisturbEnd",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "DoNotDisturbStart",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailNewsletter",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailOrderUpdates",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailPriceAlerts",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailProductReviews",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailPromotions",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "PushEnabled",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "PushSound",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "PushVibration",
                table: "CustomerNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CustomerNotificationPreferences");

            migrationBuilder.RenameColumn(
                name: "Timezone",
                table: "CustomerNotificationPreferences",
                newName: "NotificationType");

            migrationBuilder.RenameColumn(
                name: "SmsPromotions",
                table: "CustomerNotificationPreferences",
                newName: "EmailEnabled");

            migrationBuilder.RenameColumn(
                name: "SmsOrderUpdates",
                table: "CustomerNotificationPreferences",
                newName: "BrowserEnabled");

            migrationBuilder.RenameColumn(
                name: "SmsDeliveryAlerts",
                table: "CustomerNotificationPreferences",
                newName: "AppEnabled");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CustomerNotificationPreferences",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "CustomerNotificationPreferences",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationPreferences_CustomerId_NotificationType",
                table: "CustomerNotificationPreferences",
                columns: new[] { "CustomerId", "NotificationType" },
                unique: true);
        }
    }
}
