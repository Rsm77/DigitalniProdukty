using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyGroupRole : Migration
    {
        /// <inheritdoc />
        // Historická změna: přidává sloupec `Role` do KeyGroups a seeduje hodnotu pro Admin pool.
        // (Později bylo odstraněno, viz navazující migrace.)
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "KeyGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Distributor");

            // Zajistí, že seedovaný Admin pool má správnou roli.
            migrationBuilder.Sql(
                "UPDATE [KeyGroups] SET [Role] = 'Admin' WHERE [Id] = '00000000-0000-0000-0000-000000000001'");
        }

        /// <inheritdoc />
        // Vrátí změny z `Up` zpět (odebere sloupec `Role`).
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "KeyGroups");
        }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: historická úprava KeyGroups (pokus o explicitní „roli“ skupiny).
- Poznámka:
    - Následná migrace `RemoveKeyGroupRole` sloupec odstraní.
*/
