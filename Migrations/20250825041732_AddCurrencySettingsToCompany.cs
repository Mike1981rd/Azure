using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencySettingsToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyBase",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrencyLockedUntil",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyRoundingRuleJson",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnabledCurrenciesJson",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualRatesJson",
                table: "Companies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyBase",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CurrencyLockedUntil",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CurrencyRoundingRuleJson",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "EnabledCurrenciesJson",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ManualRatesJson",
                table: "Companies");
        }
    }
}
