using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialNumberOwnerUser : Migration
    {
        /// <inheritdoc />
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
