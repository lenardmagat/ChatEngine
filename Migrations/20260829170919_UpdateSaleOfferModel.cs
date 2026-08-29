using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSaleOfferModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SaleOffers_ItemId",
                table: "SaleOffers",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOffers_Products_ItemId",
                table: "SaleOffers",
                column: "ItemId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOffers_Products_ItemId",
                table: "SaleOffers");

            migrationBuilder.DropIndex(
                name: "IX_SaleOffers_ItemId",
                table: "SaleOffers");
        }
    }
}
