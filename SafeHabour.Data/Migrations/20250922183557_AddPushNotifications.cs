using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeHabour.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresAction = table.Column<bool>(type: "bit", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("5066e6e3-cbc6-48f4-87dc-25f8bd29007b"), null, new DateTime(2025, 9, 22, 18, 35, 57, 46, DateTimeKind.Utc).AddTicks(6340), "Administrator with full system access", true, "Admin", "ADMIN", null },
                    { new Guid("ac9068f9-f8f6-4fce-9402-888a25d5558f"), null, new DateTime(2025, 9, 22, 18, 35, 57, 46, DateTimeKind.Utc).AddTicks(6340), "Client who posts jobs and hires service workers", true, "Client", "CLIENT", null },
                    { new Guid("e2518fd9-b128-46bf-b46c-7d697bbca50e"), null, new DateTime(2025, 9, 22, 18, 35, 57, 46, DateTimeKind.Utc).AddTicks(6340), "Service worker who applies for and completes jobs", true, "ServiceWorker", "SERVICEWORKER", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_CreatedAt",
                table: "PushNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_ExpiresAt",
                table: "PushNotifications",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_UserId",
                table: "PushNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_UserId_IsRead",
                table: "PushNotifications",
                columns: new[] { "UserId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushNotifications");

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
    }
}
