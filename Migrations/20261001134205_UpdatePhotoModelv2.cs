using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhotoModelv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Photos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Photos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PhotoId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhotoId",
                table: "Users",
                column: "PhotoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Photos_PhotoId",
                table: "Users",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "PhotoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Photos_PhotoId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhotoId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Photos");
        }
    }
}
