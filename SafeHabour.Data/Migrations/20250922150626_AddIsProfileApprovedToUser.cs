using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeHabour.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsProfileApprovedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("69e0ebae-cee0-42e1-8421-f6db376bed4a"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("9044a6c7-aa57-4297-92c5-f4c731f926fc"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("aef7c927-cbd5-467d-98a5-0d398514f7a4"));

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileApproved",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("0005da27-e664-4ba7-ae3a-4a629c24b1d6"), null, new DateTime(2025, 9, 22, 15, 6, 26, 201, DateTimeKind.Utc).AddTicks(7780), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null },
                    { new Guid("62ec4f9c-d7a2-4677-ac68-aa990ad17529"), null, new DateTime(2025, 9, 22, 15, 6, 26, 201, DateTimeKind.Utc).AddTicks(7780), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("dde22e5e-c41d-423e-a5a9-d222f975bfe0"), null, new DateTime(2025, 9, 22, 15, 6, 26, 201, DateTimeKind.Utc).AddTicks(7780), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("0005da27-e664-4ba7-ae3a-4a629c24b1d6"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("62ec4f9c-d7a2-4677-ac68-aa990ad17529"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("dde22e5e-c41d-423e-a5a9-d222f975bfe0"));

            migrationBuilder.DropColumn(
                name: "IsProfileApproved",
                table: "Users");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("69e0ebae-cee0-42e1-8421-f6db376bed4a"), null, new DateTime(2025, 9, 22, 14, 53, 53, 652, DateTimeKind.Utc).AddTicks(1750), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null },
                    { new Guid("9044a6c7-aa57-4297-92c5-f4c731f926fc"), null, new DateTime(2025, 9, 22, 14, 53, 53, 652, DateTimeKind.Utc).AddTicks(1760), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("aef7c927-cbd5-467d-98a5-0d398514f7a4"), null, new DateTime(2025, 9, 22, 14, 53, 53, 652, DateTimeKind.Utc).AddTicks(1750), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null }
                });
        }
    }
}
