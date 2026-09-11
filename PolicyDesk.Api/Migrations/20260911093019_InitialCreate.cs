using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyDesk.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerIdentifier = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PolicyType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Insurer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillingFrequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Deductible = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedAgent = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ClaimReference = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CurrentStatus",
                table: "Policies",
                column: "CurrentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CustomerIdentifier",
                table: "Policies",
                column: "CustomerIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_EffectiveDate",
                table: "Policies",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ExpirationDate",
                table: "Policies",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Insurer",
                table: "Policies",
                column: "Insurer");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyNumber",
                table: "Policies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyType",
                table: "Policies",
                column: "PolicyType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Policies");
        }
    }
}
