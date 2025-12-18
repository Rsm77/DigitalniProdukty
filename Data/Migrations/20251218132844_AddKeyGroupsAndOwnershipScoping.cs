using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyGroupsAndOwnershipScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "SerialNumbers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.CreateTable(
                name: "KeyGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyGroups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "KeyGroups",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "Admin pool" });

            migrationBuilder.CreateTable(
                name: "SerialNumberGroupAudits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumberId = table.Column<int>(type: "int", nullable: false),
                    FromGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumberGroupAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerialNumberGroupAudits_SerialNumbers_SerialNumberId",
                        column: x => x.SerialNumberId,
                        principalTable: "SerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KeyGroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyGroupMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KeyGroupMembers_KeyGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "KeyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_GroupId",
                table: "SerialNumbers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyGroupMembers_GroupId_UserId",
                table: "KeyGroupMembers",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeyGroupMembers_UserId",
                table: "KeyGroupMembers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeyGroups_Name",
                table: "KeyGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumberGroupAudits_SerialNumberId_ChangedAt",
                table: "SerialNumberGroupAudits",
                columns: new[] { "SerialNumberId", "ChangedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_SerialNumbers_KeyGroups_GroupId",
                table: "SerialNumbers",
                column: "GroupId",
                principalTable: "KeyGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerialNumbers_KeyGroups_GroupId",
                table: "SerialNumbers");

            migrationBuilder.DropTable(
                name: "KeyGroupMembers");

            migrationBuilder.DropTable(
                name: "SerialNumberGroupAudits");

            migrationBuilder.DropTable(
                name: "KeyGroups");

            migrationBuilder.DropIndex(
                name: "IX_SerialNumbers_GroupId",
                table: "SerialNumbers");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "SerialNumbers");
        }
    }
}
