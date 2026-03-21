using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceTeaShop.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldCheckoutInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 21, 6, 52, 42, 163, DateTimeKind.Utc).AddTicks(5923), "$2a$11$VdiQQU2Vc/xGir3YPk5Zuu1NFqZzUrtdwxYu1okEu8W93InNYymVW" });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 21, 6, 52, 42, 163, DateTimeKind.Utc).AddTicks(5931), "$2a$11$VdiQQU2Vc/xGir3YPk5Zuu1NFqZzUrtdwxYu1okEu8W93InNYymVW" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 19, 6, 56, 30, 567, DateTimeKind.Utc).AddTicks(4601), "$2a$11$CANVHpzUlqqeGvgB3ziU/.Kwv/Xd4Wty40I6yd6BmuTUFPN6XapVS" });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 19, 6, 56, 30, 567, DateTimeKind.Utc).AddTicks(4610), "$2a$11$CANVHpzUlqqeGvgB3ziU/.Kwv/Xd4Wty40I6yd6BmuTUFPN6XapVS" });
        }
    }
}
