using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceTeaShop.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddoninTBL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AddonId",
                table: "OrderDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AddonId",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Addon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addon", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_AddonId",
                table: "OrderDetails",
                column: "AddonId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_AddonId",
                table: "CartItems",
                column: "AddonId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Addon_AddonId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Addon_AddonId",
                table: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Addon");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_AddonId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_AddonId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "AddonId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "AddonId",
                table: "CartItems");
        }
    }
}
