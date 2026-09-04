using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSaleOfferModel_for_breaking_it_down_into_to_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "SaleOffers");

            migrationBuilder.AddColumn<int>(
                name: "SellerUserId",
                table: "SaleOffers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SaleOffers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE ""SaleOffers"" s
                SET ""SellerUserId"" = p.""OwnerUserId""
                FROM ""Products"" p
                WHERE s.""ItemId"" = p.""Id"";

                DELETE FROM ""SaleOffers"" WHERE ""SellerUserId"" = 0 OR ""SellerUserId"" NOT IN (SELECT ""UserId"" FROM ""Users"");
            ");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOffers_SellerUserId",
                table: "SaleOffers",
                column: "SellerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOffers_Users_SellerUserId",
                table: "SaleOffers",
                column: "SellerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOffers_Users_SellerUserId",
                table: "SaleOffers");

            migrationBuilder.DropIndex(
                name: "IX_SaleOffers_SellerUserId",
                table: "SaleOffers");

            migrationBuilder.DropColumn(
                name: "SellerUserId",
                table: "SaleOffers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SaleOffers");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "SaleOffers",
                type: "integer",
                nullable: true);
        }
    }
}
