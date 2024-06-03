using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BimshireStore.Services.ShoppingCartAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsFromCardHeaderInShoppingCartAPIIDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CartHeaders");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CartHeaders");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "CartHeaders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);
        }
    }
}
