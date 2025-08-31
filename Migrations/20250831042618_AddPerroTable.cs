using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPerroTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversations_Companies_CompanyId",
                table: "WhatsAppConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversations_Customers_CustomerId",
                table: "WhatsAppConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversations_Users_AssignedUserId",
                table: "WhatsAppConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_Customers_CustomerId",
                table: "WhatsAppMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_Users_RepliedByUserId",
                table: "WhatsAppMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_WhatsAppConversations_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropTable(
                name: "GreenApiWhatsAppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_CompanyId_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_Direction",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_From",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_ReadAt",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_Status",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_Timestamp",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_To",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_TwilioSid",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConfigs_CompanyId",
                table: "WhatsAppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConfigs_IsActive",
                table: "WhatsAppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConfigs_WhatsAppPhoneNumber",
                table: "WhatsAppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ContactNotificationSettings_CompanyId",
                table: "ContactNotificationSettings");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_CreatedAt",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_Email",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_IsNotificationSent",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_Status",
                table: "ContactMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhatsAppConversations",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_CompanyId_CustomerPhone_BusinessPhone",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_LastMessageAt",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_Priority",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_Status",
                table: "WhatsAppConversations");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversations_UnreadCount",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "NewsletterSubscribers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "NewsletterSubscribers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmedAt",
                table: "NewsletterSubscribers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeToken",
                table: "NewsletterSubscribers");

            migrationBuilder.RenameTable(
                name: "WhatsAppConversations",
                newName: "WhatsAppConversation");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversations_CustomerId",
                table: "WhatsAppConversation",
                newName: "IX_WhatsAppConversation_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversations_AssignedUserId",
                table: "WhatsAppConversation",
                newName: "IX_WhatsAppConversation_AssignedUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WhatsAppMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "WhatsAppMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WhatsAppMessages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "received");

            migrationBuilder.AlterColumn<string>(
                name: "MessageType",
                table: "WhatsAppMessages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Direction",
                table: "WhatsAppMessages",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "inbound");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WhatsAppMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WhatsAppConfigs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "TwilioAuthToken",
                table: "WhatsAppConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "TwilioAccountSid",
                table: "WhatsAppConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WhatsAppConfigs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "GreenApiInstanceId",
                table: "WhatsAppConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GreenApiToken",
                table: "WhatsAppConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GreenApiTokenMask",
                table: "WhatsAppConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "WhatsAppConfigs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TwilioAccountSidMask",
                table: "WhatsAppConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwilioAuthTokenMask",
                table: "WhatsAppConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContactNotificationSettings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "ToastSuccessMessage",
                table: "ContactNotificationSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValue: "Message sent successfully!");

            migrationBuilder.AlterColumn<string>(
                name: "ToastErrorMessage",
                table: "ContactNotificationSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValue: "Error sending message. Please try again.");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationEmailAddress",
                table: "ContactNotificationSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "EmailSubjectTemplate",
                table: "ContactNotificationSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldDefaultValue: "New Contact Message from {name}");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContactNotificationSettings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ContactMessages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "unread");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ContactMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContactMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WhatsAppConversation",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WhatsAppConversation",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "active");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "WhatsAppConversation",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "WhatsAppConversation",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "normal");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WhatsAppConversation",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhatsAppConversation",
                table: "WhatsAppConversation",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Perros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Raza = table.Column<string>(type: "text", nullable: false),
                    Edad = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perros", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_CompanyId",
                table: "WhatsAppMessages",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConfigs_CompanyId",
                table: "WhatsAppConfigs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactNotificationSettings_CompanyId",
                table: "ContactNotificationSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversation_CompanyId",
                table: "WhatsAppConversation",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversation_Companies_CompanyId",
                table: "WhatsAppConversation",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversation_Customers_CustomerId",
                table: "WhatsAppConversation",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversation_Users_AssignedUserId",
                table: "WhatsAppConversation",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_Customers_CustomerId",
                table: "WhatsAppMessages",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_Users_RepliedByUserId",
                table: "WhatsAppMessages",
                column: "RepliedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_WhatsAppConversation_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId",
                principalTable: "WhatsAppConversation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversation_Companies_CompanyId",
                table: "WhatsAppConversation");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversation_Customers_CustomerId",
                table: "WhatsAppConversation");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppConversation_Users_AssignedUserId",
                table: "WhatsAppConversation");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_Customers_CustomerId",
                table: "WhatsAppMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_Users_RepliedByUserId",
                table: "WhatsAppMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppMessages_WhatsAppConversation_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropTable(
                name: "Perros");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppMessages_CompanyId",
                table: "WhatsAppMessages");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConfigs_CompanyId",
                table: "WhatsAppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ContactNotificationSettings_CompanyId",
                table: "ContactNotificationSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhatsAppConversation",
                table: "WhatsAppConversation");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppConversation_CompanyId",
                table: "WhatsAppConversation");

            migrationBuilder.DropColumn(
                name: "GreenApiInstanceId",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "GreenApiToken",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "GreenApiTokenMask",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "TwilioAccountSidMask",
                table: "WhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "TwilioAuthTokenMask",
                table: "WhatsAppConfigs");

            migrationBuilder.RenameTable(
                name: "WhatsAppConversation",
                newName: "WhatsAppConversations");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversation_CustomerId",
                table: "WhatsAppConversations",
                newName: "IX_WhatsAppConversations_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversation_AssignedUserId",
                table: "WhatsAppConversations",
                newName: "IX_WhatsAppConversations_AssignedUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WhatsAppMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "WhatsAppMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WhatsAppMessages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "received",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "MessageType",
                table: "WhatsAppMessages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "text",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Direction",
                table: "WhatsAppMessages",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "inbound",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WhatsAppMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WhatsAppConfigs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "TwilioAuthToken",
                table: "WhatsAppConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TwilioAccountSid",
                table: "WhatsAppConfigs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WhatsAppConfigs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "NewsletterSubscribers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "NewsletterSubscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmedAt",
                table: "NewsletterSubscribers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnsubscribeToken",
                table: "NewsletterSubscribers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContactNotificationSettings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "ToastSuccessMessage",
                table: "ContactNotificationSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Message sent successfully!",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ToastErrorMessage",
                table: "ContactNotificationSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Error sending message. Please try again.",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationEmailAddress",
                table: "ContactNotificationSettings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailSubjectTemplate",
                table: "ContactNotificationSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "New Contact Message from {name}",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContactNotificationSettings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ContactMessages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "unread",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ContactMessages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContactMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WhatsAppConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WhatsAppConversations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "WhatsAppConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "WhatsAppConversations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "normal",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WhatsAppConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhatsAppConversations",
                table: "WhatsAppConversations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GreenApiWhatsAppConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdditionalSettings = table.Column<string>(type: "jsonb", nullable: true),
                    ApiToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AutoAcknowledgeMessages = table.Column<bool>(type: "boolean", nullable: false),
                    AutoReplyMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BlacklistedNumbers = table.Column<string>(type: "jsonb", nullable: true),
                    BusinessHoursEnd = table.Column<TimeSpan>(type: "interval", nullable: true),
                    BusinessHoursStart = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EnableWebhook = table.Column<bool>(type: "boolean", nullable: false),
                    InstanceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastTestResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PollingIntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    RateLimitSettings = table.Column<string>(type: "jsonb", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    WebhookUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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
                name: "IX_WhatsAppMessages_CompanyId_ConversationId",
                table: "WhatsAppMessages",
                columns: new[] { "CompanyId", "ConversationId" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_Direction",
                table: "WhatsAppMessages",
                column: "Direction");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_From",
                table: "WhatsAppMessages",
                column: "From");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_ReadAt",
                table: "WhatsAppMessages",
                column: "ReadAt",
                filter: "\"ReadAt\" IS NULL AND \"Direction\" = 'inbound'");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_Status",
                table: "WhatsAppMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_Timestamp",
                table: "WhatsAppMessages",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_To",
                table: "WhatsAppMessages",
                column: "To");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_TwilioSid",
                table: "WhatsAppMessages",
                column: "TwilioSid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConfigs_CompanyId",
                table: "WhatsAppConfigs",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConfigs_IsActive",
                table: "WhatsAppConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConfigs_WhatsAppPhoneNumber",
                table: "WhatsAppConfigs",
                column: "WhatsAppPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ContactNotificationSettings_CompanyId",
                table: "ContactNotificationSettings",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CreatedAt",
                table: "ContactMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_Email",
                table: "ContactMessages",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_IsNotificationSent",
                table: "ContactMessages",
                column: "IsNotificationSent");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_Status",
                table: "ContactMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_CompanyId_CustomerPhone_BusinessPhone",
                table: "WhatsAppConversations",
                columns: new[] { "CompanyId", "CustomerPhone", "BusinessPhone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_LastMessageAt",
                table: "WhatsAppConversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_Priority",
                table: "WhatsAppConversations",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_Status",
                table: "WhatsAppConversations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversations_UnreadCount",
                table: "WhatsAppConversations",
                column: "UnreadCount",
                filter: "\"UnreadCount\" > 0");

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

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversations_Companies_CompanyId",
                table: "WhatsAppConversations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversations_Customers_CustomerId",
                table: "WhatsAppConversations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversations_Users_AssignedUserId",
                table: "WhatsAppConversations",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_Customers_CustomerId",
                table: "WhatsAppMessages",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_Users_RepliedByUserId",
                table: "WhatsAppMessages",
                column: "RepliedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_WhatsAppConversations_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId",
                principalTable: "WhatsAppConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
