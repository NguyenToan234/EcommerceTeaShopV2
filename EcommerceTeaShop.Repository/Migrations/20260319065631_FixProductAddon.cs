using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceTeaShop.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixProductAddon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Addon_AddonId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Addon_AddonId",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addon",
                table: "Addon");

            migrationBuilder.RenameTable(
                name: "Addon",
                newName: "Addons");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Addons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Addons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addons",
                table: "Addons",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductAddons",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAddons", x => new { x.ProductId, x.AddonId });
                    table.ForeignKey(
                        name: "FK_ProductAddons_Addons_AddonId",
                        column: x => x.AddonId,
                        principalTable: "Addons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAddons_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddons_AddonId",
                table: "ProductAddons",
                column: "AddonId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Addons_AddonId",
                table: "CartItems",
                column: "AddonId",
                principalTable: "Addons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Addons_AddonId",
                table: "OrderDetails",
                column: "AddonId",
                principalTable: "Addons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Addons_AddonId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Addons_AddonId",
                table: "OrderDetails");

            migrationBuilder.DropTable(
                name: "ProductAddons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addons",
                table: "Addons");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Addons");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Addons");

            migrationBuilder.RenameTable(
                name: "Addons",
                newName: "Addon");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addon",
                table: "Addon",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 16, 15, 4, 19, 665, DateTimeKind.Utc).AddTicks(5262), "$2a$11$1RIuSI9LoDehzwkHA67c8.sOtwuNH56JKxt7qB1R/m8OddR4rUf0y" });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 16, 15, 4, 19, 665, DateTimeKind.Utc).AddTicks(5269), "$2a$11$1RIuSI9LoDehzwkHA67c8.sOtwuNH56JKxt7qB1R/m8OddR4rUf0y" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Addon_AddonId",
                table: "CartItems",
                column: "AddonId",
                principalTable: "Addon",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Addon_AddonId",
                table: "OrderDetails",
                column: "AddonId",
                principalTable: "Addon",
                principalColumn: "Id");
        }
    }
}
