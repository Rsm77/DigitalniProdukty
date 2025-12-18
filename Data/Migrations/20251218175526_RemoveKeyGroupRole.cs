using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKeyGroupRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "KeyGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "KeyGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Distributor");
        }
    }
}
