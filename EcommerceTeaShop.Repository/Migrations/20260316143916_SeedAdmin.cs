using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceTeaShop.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "Email", "FullName", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 16, 14, 39, 15, 316, DateTimeKind.Utc).AddTicks(9973), "admin@teashop.com", "Super Admin", "$2a$11$bTdnecxozSnr8zYxYV7yc.foxJjApoeWPhA4h821.DMukCvIfYPGm" });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "EmailOtp", "EmailOtpExpiry", "EmailVerified", "FullName", "GoogleId", "IsDeleted", "LastLoginAt", "PasswordHash", "Phone", "Role", "Status", "UpdatedAt" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), null, new DateTime(2026, 3, 16, 14, 39, 15, 316, DateTimeKind.Utc).AddTicks(9983), "admin2@teashop.com", null, null, true, "Second Admin", null, false, null, "$2a$11$bTdnecxozSnr8zYxYV7yc.foxJjApoeWPhA4h821.DMukCvIfYPGm", null, "Admin", "Active", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "Email", "FullName", "PasswordHash" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "hibana664@gmail.com", "System Admin", "$2a$11$K9LQmT9J5tVn3h6r1mB6yOBXlJfK1z2Yq3cE7U8dR9sT0uV1wX2yG" });
        }
    }
}
