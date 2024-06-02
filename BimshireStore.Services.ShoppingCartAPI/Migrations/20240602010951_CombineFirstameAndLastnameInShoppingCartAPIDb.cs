using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BimshireStore.Services.ShoppingCartAPI.Migrations
{
    /// <inheritdoc />
    public partial class CombineFirstameAndLastnameInShoppingCartAPIDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Firstname",
                table: "CartHeaders");

            migrationBuilder.RenameColumn(
                name: "Lastname",
                table: "CartHeaders",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CartHeaders",
                newName: "Lastname");

            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);
        }
    }
}
