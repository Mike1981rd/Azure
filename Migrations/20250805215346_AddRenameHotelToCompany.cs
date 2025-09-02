using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRenameHotelToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NavigationMenus_Hotels_HotelId",
                table: "NavigationMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentProviders_Hotels_HotelId",
                table: "PaymentProviders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Hotels_HotelId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductShoppingCarts_Hotels_HotelId",
                table: "ProductShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomReservationCarts_Hotels_HotelId",
                table: "RoomReservationCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Hotels_HotelId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ThemeSettings_Hotels_HotelId",
                table: "ThemeSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Hotels_HotelId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsitePages_Hotels_HotelId",
                table: "WebsitePages");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "WebsitePages",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePages_HotelId",
                table: "WebsitePages",
                newName: "IX_WebsitePages_CompanyId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "Users",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_HotelId",
                table: "Users",
                newName: "IX_Users_CompanyId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "ThemeSettings",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_ThemeSettings_HotelId",
                table: "ThemeSettings",
                newName: "IX_ThemeSettings_CompanyId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "Rooms",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_HotelId",
                table: "Rooms",
                newName: "IX_Rooms_CompanyId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "RoomReservationCarts",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_RoomReservationCarts_HotelId",
                table: "RoomReservationCarts",
                newName: "IX_RoomReservationCarts_CompanyId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "ProductShoppingCarts",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductShoppingCarts_HotelId",
                table: "ProductShoppingCarts",
                newName: "IX_ProductShoppingCarts_CompanyId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "Products",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_HotelId",
                table: "Products",
                newName: "IX_Products_CompanyId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "PaymentProviders",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentProviders_HotelId_Provider",
                table: "PaymentProviders",
                newName: "IX_PaymentProviders_CompanyId_Provider");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "NavigationMenus",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_NavigationMenus_HotelId",
                table: "NavigationMenus",
                newName: "IX_NavigationMenus_CompanyId");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: true),
                    CustomDomain = table.Column<string>(type: "text", nullable: true),
                    Subdomain = table.Column<string>(type: "text", nullable: true),
                    Logo = table.Column<string>(type: "text", nullable: true),
                    PrimaryColor = table.Column<string>(type: "text", nullable: true),
                    SecondaryColor = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    SenderEmail = table.Column<string>(type: "text", nullable: true),
                    LegalBusinessName = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Region = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Apartment = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    MetricSystem = table.Column<string>(type: "text", nullable: true),
                    WeightUnit = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    OrderIdPrefix = table.Column<string>(type: "text", nullable: true),
                    OrderIdSuffix = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Domain",
                table: "Companies",
                column: "Domain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NavigationMenus_Companies_CompanyId",
                table: "NavigationMenus",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentProviders_Companies_CompanyId",
                table: "PaymentProviders",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductShoppingCarts_Companies_CompanyId",
                table: "ProductShoppingCarts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomReservationCarts_Companies_CompanyId",
                table: "RoomReservationCarts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Companies_CompanyId",
                table: "Rooms",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThemeSettings_Companies_CompanyId",
                table: "ThemeSettings",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companies_CompanyId",
                table: "Users",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WebsitePages_Companies_CompanyId",
                table: "WebsitePages",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NavigationMenus_Companies_CompanyId",
                table: "NavigationMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentProviders_Companies_CompanyId",
                table: "PaymentProviders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductShoppingCarts_Companies_CompanyId",
                table: "ProductShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomReservationCarts_Companies_CompanyId",
                table: "RoomReservationCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Companies_CompanyId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ThemeSettings_Companies_CompanyId",
                table: "ThemeSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companies_CompanyId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsitePages_Companies_CompanyId",
                table: "WebsitePages");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "WebsitePages",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_WebsitePages_CompanyId",
                table: "WebsitePages",
                newName: "IX_WebsitePages_HotelId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Users",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                newName: "IX_Users_HotelId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "ThemeSettings",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_ThemeSettings_CompanyId",
                table: "ThemeSettings",
                newName: "IX_ThemeSettings_HotelId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Rooms",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_CompanyId",
                table: "Rooms",
                newName: "IX_Rooms_HotelId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "RoomReservationCarts",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_RoomReservationCarts_CompanyId",
                table: "RoomReservationCarts",
                newName: "IX_RoomReservationCarts_HotelId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "ProductShoppingCarts",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductShoppingCarts_CompanyId",
                table: "ProductShoppingCarts",
                newName: "IX_ProductShoppingCarts_HotelId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Products",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CompanyId",
                table: "Products",
                newName: "IX_Products_HotelId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "PaymentProviders",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentProviders_CompanyId_Provider",
                table: "PaymentProviders",
                newName: "IX_PaymentProviders_HotelId_Provider");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "NavigationMenus",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_NavigationMenus_CompanyId",
                table: "NavigationMenus",
                newName: "IX_NavigationMenus_HotelId");

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Apartment = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    CustomDomain = table.Column<string>(type: "text", nullable: true),
                    Domain = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LegalBusinessName = table.Column<string>(type: "text", nullable: true),
                    Logo = table.Column<string>(type: "text", nullable: true),
                    MetricSystem = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OrderIdPrefix = table.Column<string>(type: "text", nullable: true),
                    OrderIdSuffix = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    PrimaryColor = table.Column<string>(type: "text", nullable: true),
                    Region = table.Column<string>(type: "text", nullable: true),
                    SecondaryColor = table.Column<string>(type: "text", nullable: true),
                    SenderEmail = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    Subdomain = table.Column<string>(type: "text", nullable: true),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    WeightUnit = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Domain",
                table: "Hotels",
                column: "Domain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NavigationMenus_Hotels_HotelId",
                table: "NavigationMenus",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentProviders_Hotels_HotelId",
                table: "PaymentProviders",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Hotels_HotelId",
                table: "Products",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductShoppingCarts_Hotels_HotelId",
                table: "ProductShoppingCarts",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomReservationCarts_Hotels_HotelId",
                table: "RoomReservationCarts",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Hotels_HotelId",
                table: "Rooms",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThemeSettings_Hotels_HotelId",
                table: "ThemeSettings",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Hotels_HotelId",
                table: "Users",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WebsitePages_Hotels_HotelId",
                table: "WebsitePages",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
