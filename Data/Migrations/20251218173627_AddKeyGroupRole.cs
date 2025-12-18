using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyGroupRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "KeyGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Distributor");

            // Ensure seeded Admin pool keeps its intended role.
            migrationBuilder.Sql(
                "UPDATE [KeyGroups] SET [Role] = 'Admin' WHERE [Id] = '00000000-0000-0000-0000-000000000001'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "KeyGroups");
        }
    }
}
