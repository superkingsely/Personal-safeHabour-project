using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeHabour.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("3834f013-3d07-4b10-915f-7f47c369d4cd"), null, new DateTime(2025, 9, 23, 11, 39, 17, 78, DateTimeKind.Utc).AddTicks(4730), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("8a5a63e3-8f4e-447b-b8ba-d59ab908b0c2"), null, new DateTime(2025, 9, 23, 11, 39, 17, 78, DateTimeKind.Utc).AddTicks(4730), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null },
                    { new Guid("f5be8be6-6bf8-4645-9b09-d123fa9f2562"), null, new DateTime(2025, 9, 23, 11, 39, 17, 78, DateTimeKind.Utc).AddTicks(4730), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("3834f013-3d07-4b10-915f-7f47c369d4cd"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("8a5a63e3-8f4e-447b-b8ba-d59ab908b0c2"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("f5be8be6-6bf8-4645-9b09-d123fa9f2562"));

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Users");

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
    }
}
