using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOfferModels_To_Fit_In_UpdateProduceStatus_pattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Offers_TradeOfferId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Chatrooms_RoomId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Offers_ParentOfferId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Users_ProposedByUserId",
                table: "Offers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Offers",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "ItemRequested",
                table: "Offers");

            migrationBuilder.RenameTable(
                name: "Offers",
                newName: "TradeOffers");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_RoomId",
                table: "TradeOffers",
                newName: "IX_TradeOffers_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_ProposedByUserId",
                table: "TradeOffers",
                newName: "IX_TradeOffers_ProposedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_ParentOfferId",
                table: "TradeOffers",
                newName: "IX_TradeOffers_ParentOfferId");

            migrationBuilder.AddColumn<int>(
                name: "ItemRequestedId",
                table: "TradeOffers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TradeOffers",
                table: "TradeOffers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_ItemRequestedId",
                table: "TradeOffers",
                column: "ItemRequestedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_TradeOffers_TradeOfferId",
                table: "Messages",
                column: "TradeOfferId",
                principalTable: "TradeOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffers_Chatrooms_RoomId",
                table: "TradeOffers",
                column: "RoomId",
                principalTable: "Chatrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffers_Products_ItemRequestedId",
                table: "TradeOffers",
                column: "ItemRequestedId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffers_TradeOffers_ParentOfferId",
                table: "TradeOffers",
                column: "ParentOfferId",
                principalTable: "TradeOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffers_Users_ProposedByUserId",
                table: "TradeOffers",
                column: "ProposedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_TradeOffers_TradeOfferId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffers_Chatrooms_RoomId",
                table: "TradeOffers");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffers_Products_ItemRequestedId",
                table: "TradeOffers");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffers_TradeOffers_ParentOfferId",
                table: "TradeOffers");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffers_Users_ProposedByUserId",
                table: "TradeOffers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TradeOffers",
                table: "TradeOffers");

            migrationBuilder.DropIndex(
                name: "IX_TradeOffers_ItemRequestedId",
                table: "TradeOffers");

            migrationBuilder.DropColumn(
                name: "ItemRequestedId",
                table: "TradeOffers");

            migrationBuilder.RenameTable(
                name: "TradeOffers",
                newName: "Offers");

            migrationBuilder.RenameIndex(
                name: "IX_TradeOffers_RoomId",
                table: "Offers",
                newName: "IX_Offers_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_TradeOffers_ProposedByUserId",
                table: "Offers",
                newName: "IX_Offers_ProposedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TradeOffers_ParentOfferId",
                table: "Offers",
                newName: "IX_Offers_ParentOfferId");

            migrationBuilder.AddColumn<string>(
                name: "ItemRequested",
                table: "Offers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offers",
                table: "Offers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Offers_TradeOfferId",
                table: "Messages",
                column: "TradeOfferId",
                principalTable: "Offers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Chatrooms_RoomId",
                table: "Offers",
                column: "RoomId",
                principalTable: "Chatrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Offers_ParentOfferId",
                table: "Offers",
                column: "ParentOfferId",
                principalTable: "Offers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Users_ProposedByUserId",
                table: "Offers",
                column: "ProposedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
