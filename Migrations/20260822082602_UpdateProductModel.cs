using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsForSale",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsForTrade",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ProductQuantity",
                table: "Products",
                newName: "Stock");

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductAvailable",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductAvailable",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "Products",
                newName: "ProductQuantity");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsForSale",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsForTrade",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
