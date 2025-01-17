using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassLibrary.Migrations
{
    public partial class AddScreenAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScreenAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreenId = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastAccessTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreenAccesses_Screens_ScreenId",
                        column: x => x.ScreenId,
                        principalTable: "Screens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateCreated", "LastLogin", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 17, 10, 38, 43, 479, DateTimeKind.Utc).AddTicks(7870), new DateTime(2025, 1, 17, 10, 38, 43, 479, DateTimeKind.Utc).AddTicks(7870), "$2a$11$/BpOCa3/WE1mWq.KXTDym.0e01z/nXCySdOCdyVW5Q9CRPVMKmqQ." });

            migrationBuilder.CreateIndex(
                name: "IX_ScreenAccesses_ScreenId",
                table: "ScreenAccesses",
                column: "ScreenId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScreenAccesses");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateCreated", "LastLogin", "PasswordHash" },
                values: new object[] { new DateTime(2025, 1, 8, 12, 25, 36, 66, DateTimeKind.Utc).AddTicks(3000), new DateTime(2025, 1, 8, 12, 25, 36, 66, DateTimeKind.Utc).AddTicks(3000), "$2a$11$b6P/RH6fegGdzhEE.LU/IeVvodryZLAng8.JGyPoikbtBGsKpCfcO" });
        }
    }
}
