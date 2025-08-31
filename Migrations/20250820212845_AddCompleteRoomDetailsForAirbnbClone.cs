using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCompleteRoomDetailsForAirbnbClone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<JsonDocument>(
                name: "AdditionalFees",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Rooms",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "CancellationPolicy",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "CheckInInstructions",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "HighlightFeatures",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "HouseRules",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Rooms",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Rooms",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ResponseRate",
                table: "Rooms",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseTime",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "SafetyFeatures",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "SleepingArrangements",
                table: "Rooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalFees",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CancellationPolicy",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CheckInInstructions",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "HighlightFeatures",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "HouseRules",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ResponseRate",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ResponseTime",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "SafetyFeatures",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "SleepingArrangements",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Rooms");
        }
    }
}
