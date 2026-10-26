using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadEntryv5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_PhotoId",
                table: "Products",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Photos_PhotoId",
                table: "Products",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "PhotoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Photos_PhotoId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PhotoId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "Products");
        }
    }
}
