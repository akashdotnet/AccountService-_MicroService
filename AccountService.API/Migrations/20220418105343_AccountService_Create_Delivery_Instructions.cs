using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Create_Delivery_Instructions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Audit_CustomerDeliveryInstructions",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    SubdivisionName = table.Column<string>(type: "text", nullable: true),
                    HomeAccessDetails = table.Column<string>(type: "text", nullable: true),
                    PetInformation = table.Column<string>(type: "text", nullable: true),
                    HealthAndSafetyInformation = table.Column<string>(type: "text", nullable: true),
                    PoolOrEquipmentNotes = table.Column<string>(type: "text", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_CustomerDeliveryInstructions", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerDeliveryInstructions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    SubdivisionName = table.Column<string>(type: "text", nullable: true),
                    HomeAccessDetails = table.Column<string>(type: "text", nullable: true),
                    PetInformation = table.Column<string>(type: "text", nullable: true),
                    HealthAndSafetyInformation = table.Column<string>(type: "text", nullable: true),
                    PoolOrEquipmentNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDeliveryInstructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDeliveryInstructions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDeliveryInstructions_CustomerId",
                table: "CustomerDeliveryInstructions",
                column: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit_CustomerDeliveryInstructions");

            migrationBuilder.DropTable(
                name: "CustomerDeliveryInstructions");
        }
    }
}
