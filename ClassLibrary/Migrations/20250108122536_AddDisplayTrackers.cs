using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassLibrary.Migrations
{
    public partial class AddDisplayTrackers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "MenuItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DisplayTrackers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreenId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    DisplayedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayTrackers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayTrackers_Screens_ScreenId",
                        column: x => x.ScreenId,
                        principalTable: "Screens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateCreated", "LastLogin", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 8, 12, 25, 36, 66, DateTimeKind.Utc).AddTicks(3000), new DateTime(2025, 1, 8, 12, 25, 36, 66, DateTimeKind.Utc).AddTicks(3000), "$2a$11$b6P/RH6fegGdzhEE.LU/IeVvodryZLAng8.JGyPoikbtBGsKpCfcO" });

            migrationBuilder.CreateIndex(
                name: "IX_DisplayTrackers_ScreenId",
                table: "DisplayTrackers",
                column: "ScreenId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisplayTrackers");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "MenuItems",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateCreated", "LastLogin", "PasswordHash" },
                values: new object[] { new DateTime(2024, 12, 23, 15, 4, 19, 46, DateTimeKind.Utc).AddTicks(4350), new DateTime(2024, 12, 23, 15, 4, 19, 46, DateTimeKind.Utc).AddTicks(4350), "$2a$11$23WCbw5zETgCXrfUFLpnUufxUo4vRCD2iYXThIVPEX8aUlORNE2jS" });
        }
    }
}
