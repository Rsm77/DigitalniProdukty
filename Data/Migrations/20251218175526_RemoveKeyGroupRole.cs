using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKeyGroupRole : Migration
    {
        /// <inheritdoc />
        // Odstraní sloupec `Role` z tabulky KeyGroups (rollback předchozí změny).
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "KeyGroups");
        }

        /// <inheritdoc />
        // Vrátí změny z `Up` zpět (znovu přidá sloupec `Role` s defaultem).
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

/*
Podrobnosti (vazby a použité části)

- Účel: odstranění historického sloupce `KeyGroups.Role`.
- Poznámka:
    - Tato migrace existuje pro konzistenci historie a možnost rollbacku.
*/
