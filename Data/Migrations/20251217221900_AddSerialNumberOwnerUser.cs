using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialNumberOwnerUser : Migration
    {
        /// <inheritdoc />
        // Přidá volitelného vlastníka licence (`OwnerUserId`) navázaného na Identity uživatele.
        // Umožní přiřazování klíčů konkrétním end-user účtům.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "SerialNumbers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_OwnerUserId",
                table: "SerialNumbers",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SerialNumbers_AspNetUsers_OwnerUserId",
                table: "SerialNumbers",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        // Vrátí změny z `Up` zpět (odebere sloupec, index a FK).
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerialNumbers_AspNetUsers_OwnerUserId",
                table: "SerialNumbers");

            migrationBuilder.DropIndex(
                name: "IX_SerialNumbers_OwnerUserId",
                table: "SerialNumbers");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "SerialNumbers");
        }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: rozšíření licencí o vlastníka (Identity user), aby šlo klíče přiřazovat uživatelům.
- Změny:
    - `SerialNumbers.OwnerUserId` + index `IX_SerialNumbers_OwnerUserId`.
    - FK na `AspNetUsers(Id)` s `Restrict` (audit/licence se nemaže kaskádou).
- Vazby:
    - Používá `LicensingAdminController` pro assign/reassign/unassign.
*/
