using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeHabour.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("5066e6e3-cbc6-48f4-87dc-25f8bd29007b"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("ac9068f9-f8f6-4fce-9402-888a25d5558f"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("e2518fd9-b128-46bf-b46c-7d697bbca50e"));

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2c6c5f17-b586-45cf-864b-872418ce8366"), null, new DateTime(2025, 9, 23, 10, 44, 16, 225, DateTimeKind.Utc).AddTicks(5430), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("6dbcfd58-5098-44fc-ada3-6be608403e94"), null, new DateTime(2025, 9, 23, 10, 44, 16, 225, DateTimeKind.Utc).AddTicks(5430), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null },
                    { new Guid("76c7c65e-5c36-4f63-8a08-9c43d1a8843d"), null, new DateTime(2025, 9, 23, 10, 44, 16, 225, DateTimeKind.Utc).AddTicks(5430), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("2c6c5f17-b586-45cf-864b-872418ce8366"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("6dbcfd58-5098-44fc-ada3-6be608403e94"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("76c7c65e-5c36-4f63-8a08-9c43d1a8843d"));

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "Users");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("5066e6e3-cbc6-48f4-87dc-25f8bd29007b"), null, new DateTime(2025, 9, 22, 18, 35, 57, 46, DateTimeKind.Utc).AddTicks(6340), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("ac9068f9-f8f6-4fce-9402-888a25d5558f"), null, new DateTime(2025, 9, 22, 18, 35, 57, 46, DateTimeKind.Utc).AddTicks(6340), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null },
                    { new Guid("e2518fd9-b128-46bf-b46c-7d697bbca50e"), null, new DateTime(2025, 9, 22, 18, 35, 57, 46, DateTimeKind.Utc).AddTicks(6340), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null }
                });
        }
    }
}
