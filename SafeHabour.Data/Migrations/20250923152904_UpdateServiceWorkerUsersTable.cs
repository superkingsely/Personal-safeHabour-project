using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeHabour.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceWorkerUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "Services",
                table: "ServiceWorkerUsers",
                newName: "ServicesJson");

            migrationBuilder.RenameColumn(
                name: "Languages",
                table: "ServiceWorkerUsers",
                newName: "LanguagesJson");

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                table: "ServiceWorkerUsers",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ServiceWorkerUsers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ServiceWorkerUsers",
                type: "float",
                nullable: true);

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("0b827df4-e8b6-49fd-a75d-09fbf5aa0262"), null, new DateTime(2025, 9, 23, 15, 29, 3, 978, DateTimeKind.Utc).AddTicks(7820), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("2b013dc2-2b4f-476f-8668-34f7558f9274"), null, new DateTime(2025, 9, 23, 15, 29, 3, 978, DateTimeKind.Utc).AddTicks(7820), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null },
                    { new Guid("63d62fc7-26df-49e9-b3e2-6ba806c1f6b9"), null, new DateTime(2025, 9, 23, 15, 29, 3, 978, DateTimeKind.Utc).AddTicks(7820), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("0b827df4-e8b6-49fd-a75d-09fbf5aa0262"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("2b013dc2-2b4f-476f-8668-34f7558f9274"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("63d62fc7-26df-49e9-b3e2-6ba806c1f6b9"));

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ServiceWorkerUsers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ServiceWorkerUsers");

            migrationBuilder.RenameColumn(
                name: "ServicesJson",
                table: "ServiceWorkerUsers",
                newName: "Services");

            migrationBuilder.RenameColumn(
                name: "LanguagesJson",
                table: "ServiceWorkerUsers",
                newName: "Languages");

            migrationBuilder.AlterColumn<int>(
                name: "HourlyRate",
                table: "ServiceWorkerUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

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
    }
}
