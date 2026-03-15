using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceTeaShop.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addfieldinCartAndCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CouponId",
                table: "Carts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "CartItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CouponId",
                table: "Carts",
                column: "CouponId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Coupons_CouponId",
                table: "Carts",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Coupons_CouponId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_CouponId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "CartItems");
        }
    }
}
