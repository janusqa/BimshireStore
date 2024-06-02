using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BimshireStore.Services.OrderAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOrderDetailssTableToOrderDetailsTablesToOrderAPIDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetailss_OrderHeaders_OrderHeaderId",
                table: "OrderDetailss");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderDetailss",
                table: "OrderDetailss");

            migrationBuilder.RenameTable(
                name: "OrderDetailss",
                newName: "OrderDetails");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetailss_OrderHeaderId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_OrderHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderHeaderId",
                table: "OrderDetails",
                column: "OrderHeaderId",
                principalTable: "OrderHeaders",
                principalColumn: "OrderHeaderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderHeaderId",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails");

            migrationBuilder.RenameTable(
                name: "OrderDetails",
                newName: "OrderDetailss");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_OrderHeaderId",
                table: "OrderDetailss",
                newName: "IX_OrderDetailss_OrderHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderDetailss",
                table: "OrderDetailss",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetailss_OrderHeaders_OrderHeaderId",
                table: "OrderDetailss",
                column: "OrderHeaderId",
                principalTable: "OrderHeaders",
                principalColumn: "OrderHeaderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
