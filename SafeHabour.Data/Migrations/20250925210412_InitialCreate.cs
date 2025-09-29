using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeHabour.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    { new Guid("7b634711-776f-4da1-b3e3-1f7362827d70"), null, new DateTime(2025, 9, 25, 21, 4, 10, 11, DateTimeKind.Utc).AddTicks(9126), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("821b1b5b-ca1a-45d5-8cc8-41925bb5ade1"), null, new DateTime(2025, 9, 25, 21, 4, 10, 11, DateTimeKind.Utc).AddTicks(9123), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null },
                    { new Guid("a6cd8c0a-83db-4f27-ae03-2eedb2d543c6"), null, new DateTime(2025, 9, 25, 21, 4, 10, 11, DateTimeKind.Utc).AddTicks(9121), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("7b634711-776f-4da1-b3e3-1f7362827d70"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("821b1b5b-ca1a-45d5-8cc8-41925bb5ade1"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("a6cd8c0a-83db-4f27-ae03-2eedb2d543c6"));

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
