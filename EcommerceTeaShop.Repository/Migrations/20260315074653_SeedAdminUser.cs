using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceTeaShop.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "EmailOtp", "EmailOtpExpiry", "EmailVerified", "FullName", "GoogleId", "IsDeleted", "LastLoginAt", "PasswordHash", "Phone", "Role", "Status", "UpdatedAt" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "hibana664@gmail.com", null, null, true, "System Admin", null, false, null, "$2a$11$K9LQmT9J5tVn3h6r1mB6yOBXlJfK1z2Yq3cE7U8dR9sT0uV1wX2yG", null, "Admin", "Active", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
