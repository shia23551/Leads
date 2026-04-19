using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leads.Migrations
{
    /// <inheritdoc />
    public partial class CompanyTaxIdOptionalAndDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCompanies_TenantId_TaxId",
                table: "AppCompanies");

            migrationBuilder.AlterColumn<string>(
                name: "TaxId",
                table: "AppCompanies",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeCount",
                table: "AppCompanies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "CapitalAmount",
                table: "AppCompanies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_AppCompanies_TenantId_TaxId",
                table: "AppCompanies",
                columns: new[] { "TenantId", "TaxId" },
                unique: true,
                filter: "[TaxId] IS NOT NULL AND [TaxId] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCompanies_TenantId_TaxId",
                table: "AppCompanies");

            migrationBuilder.AlterColumn<string>(
                name: "TaxId",
                table: "AppCompanies",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeCount",
                table: "AppCompanies",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "CapitalAmount",
                table: "AppCompanies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_AppCompanies_TenantId_TaxId",
                table: "AppCompanies",
                columns: new[] { "TenantId", "TaxId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }
    }
}
