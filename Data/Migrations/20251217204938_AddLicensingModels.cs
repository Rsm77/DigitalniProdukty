using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalniProdukty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLicensingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HardwareIdentifier = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastIpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SerialNumberIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    MaxDevices = table.Column<int>(type: "int", nullable: false),
                    MaxActivations = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumbers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Installations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumberId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Installations_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Installations_SerialNumbers_SerialNumberId",
                        column: x => x.SerialNumberId,
                        principalTable: "SerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LicenseDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumberId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    BoundAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnboundAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LicenseDevices_SerialNumbers_SerialNumberId",
                        column: x => x.SerialNumberId,
                        principalTable: "SerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_HardwareIdentifier",
                table: "Devices",
                column: "HardwareIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Installations_DeviceId_OccurredAt",
                table: "Installations",
                columns: new[] { "DeviceId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Installations_SerialNumberId_OccurredAt",
                table: "Installations",
                columns: new[] { "SerialNumberId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseDevices_DeviceId",
                table: "LicenseDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseDevices_SerialNumberId_DeviceId",
                table: "LicenseDevices",
                columns: new[] { "SerialNumberId", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_Key",
                table: "SerialNumbers",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Installations");

            migrationBuilder.DropTable(
                name: "LicenseDevices");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "SerialNumbers");
        }
    }
}
