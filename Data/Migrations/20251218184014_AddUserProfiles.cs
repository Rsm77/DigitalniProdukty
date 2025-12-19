using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfiles : Migration
    {
        /// <inheritdoc />
        // Přidá tabulku `UserProfiles` pro profilové údaje (např. DisplayName) navázané 1:1 na uživatele.
        // FK na `AspNetUsers` je kaskádní (smazání uživatele smaže i profil).
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        // Vrátí změny z `Up` zpět (drop `UserProfiles`).
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: oddělená tabulka s profilem uživatele (UI-friendly data mimo Identity core tabulky).
- Vazby:
    - Model: `Models/Users/UserProfileModel`.
    - UI: `AuthController` (registrace), `AdminController`/`OwnerController` (editace display name).
*/
