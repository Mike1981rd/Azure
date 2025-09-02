using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomPolicyOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "CancellationPolicyOrder",
                table: "Rooms",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "HouseRulesOrder",
                table: "Rooms",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "SafetyAndPropertyOrder",
                table: "Rooms",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationPolicyOrder",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "HouseRulesOrder",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "SafetyAndPropertyOrder",
                table: "Rooms");
        }
    }
}
