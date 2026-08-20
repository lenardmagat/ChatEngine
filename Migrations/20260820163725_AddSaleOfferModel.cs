using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleOfferModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SaleOfferId",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SaleOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    ProposedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    QuantityRequested = table.Column<int>(type: "integer", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleOffers_Chatrooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Chatrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleOffers_Users_ProposedByUserId",
                        column: x => x.ProposedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SaleOfferId",
                table: "Messages",
                column: "SaleOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOffers_ProposedByUserId",
                table: "SaleOffers",
                column: "ProposedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOffers_RoomId",
                table: "SaleOffers",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_SaleOffers_SaleOfferId",
                table: "Messages",
                column: "SaleOfferId",
                principalTable: "SaleOffers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_SaleOffers_SaleOfferId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "SaleOffers");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SaleOfferId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SaleOfferId",
                table: "Messages");
        }
    }
}
