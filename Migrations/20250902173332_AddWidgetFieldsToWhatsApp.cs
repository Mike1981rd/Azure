using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWidgetFieldsToWhatsApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_WhatsAppMessages_WhatsAppConversation_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhatsAppConversation",
                table: "WhatsAppConversation");

            migrationBuilder.RenameTable(
                name: "WhatsAppConversation",
                newName: "WhatsAppConversations");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversation_CustomerId",
                table: "WhatsAppConversations",
                newName: "IX_WhatsAppConversations_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversation_CompanyId",
                table: "WhatsAppConversations",
                newName: "IX_WhatsAppConversations_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversation_AssignedUserId",
                table: "WhatsAppConversations",
                newName: "IX_WhatsAppConversations_AssignedUserId");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "WhatsAppMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "WhatsAppMessages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "WhatsAppConversations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "WhatsAppConversations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "WhatsAppConversations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhatsAppConversations",
                table: "WhatsAppConversations",
                column: "Id");

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
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppConversations_Users_AssignedUserId",
                table: "WhatsAppConversations",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppMessages_WhatsAppConversations_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId",
                principalTable: "WhatsAppConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_WhatsAppMessages_WhatsAppConversations_ConversationId",
                table: "WhatsAppMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhatsAppConversations",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "WhatsAppMessages");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "WhatsAppMessages");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "WhatsAppConversations");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "WhatsAppConversations");

            migrationBuilder.RenameTable(
                name: "WhatsAppConversations",
                newName: "WhatsAppConversation");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversations_CustomerId",
                table: "WhatsAppConversation",
                newName: "IX_WhatsAppConversation_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversations_CompanyId",
                table: "WhatsAppConversation",
                newName: "IX_WhatsAppConversation_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_WhatsAppConversations_AssignedUserId",
                table: "WhatsAppConversation",
                newName: "IX_WhatsAppConversation_AssignedUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhatsAppConversation",
                table: "WhatsAppConversation",
                column: "Id");

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
                name: "FK_WhatsAppMessages_WhatsAppConversation_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId",
                principalTable: "WhatsAppConversation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
