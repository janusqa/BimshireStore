using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BimshireStore.Services.ShoppingCartAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraUserFieldsTodShoppingCartAPIDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lastname",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "CartHeaders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CartHeaders");

            migrationBuilder.DropColumn(
                name: "Firstname",
                table: "CartHeaders");

            migrationBuilder.DropColumn(
                name: "Lastname",
                table: "CartHeaders");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "CartHeaders");
        }
    }
}
