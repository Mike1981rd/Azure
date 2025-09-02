using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomPolicyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<JsonDocument>(
                name: "CommonSpaces",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "GuestMaximum",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "RoomDetails",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "SafetyAndProperty",
                table: "Rooms",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommonSpaces",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "GuestMaximum",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "RoomDetails",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "SafetyAndProperty",
                table: "Rooms");
        }
    }
}
