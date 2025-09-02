using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBuilderAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewsForRoomSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReviewStatistics_CompanyId",
                table: "ReviewStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ReviewStatistics_CompanyId_ProductId",
                table: "ReviewStatistics");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CompanyId_ProductId",
                table: "Reviews");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "ReviewStatistics",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Reviews",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStatistics_CompanyId",
                table: "ReviewStatistics",
                column: "CompanyId",
                filter: "\"ProductId\" IS NULL AND \"RoomId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStatistics_CompanyId_ProductId",
                table: "ReviewStatistics",
                columns: new[] { "CompanyId", "ProductId" },
                unique: true,
                filter: "\"ProductId\" IS NOT NULL AND \"RoomId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStatistics_CompanyId_RoomId",
                table: "ReviewStatistics",
                columns: new[] { "CompanyId", "RoomId" },
                unique: true,
                filter: "\"RoomId\" IS NOT NULL AND \"ProductId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStatistics_RoomId",
                table: "ReviewStatistics",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CompanyId_ProductId",
                table: "Reviews",
                columns: new[] { "CompanyId", "ProductId" },
                filter: "\"ProductId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CompanyId_RoomId",
                table: "Reviews",
                columns: new[] { "CompanyId", "RoomId" },
                filter: "\"RoomId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RoomId",
                table: "Reviews",
                column: "RoomId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Review_ProductOrRoom",
                table: "Reviews",
                sql: "(\"ProductId\" IS NOT NULL AND \"RoomId\" IS NULL) OR (\"ProductId\" IS NULL AND \"RoomId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Rooms_RoomId",
                table: "Reviews",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewStatistics_Rooms_RoomId",
                table: "ReviewStatistics",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Rooms_RoomId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewStatistics_Rooms_RoomId",
                table: "ReviewStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ReviewStatistics_CompanyId",
                table: "ReviewStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ReviewStatistics_CompanyId_ProductId",
                table: "ReviewStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ReviewStatistics_CompanyId_RoomId",
                table: "ReviewStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ReviewStatistics_RoomId",
                table: "ReviewStatistics");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CompanyId_ProductId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CompanyId_RoomId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_RoomId",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Review_ProductOrRoom",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "ReviewStatistics");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStatistics_CompanyId",
                table: "ReviewStatistics",
                column: "CompanyId",
                filter: "\"ProductId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStatistics_CompanyId_ProductId",
                table: "ReviewStatistics",
                columns: new[] { "CompanyId", "ProductId" },
                unique: true,
                filter: "\"ProductId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CompanyId_ProductId",
                table: "Reviews",
                columns: new[] { "CompanyId", "ProductId" });
        }
    }
}
