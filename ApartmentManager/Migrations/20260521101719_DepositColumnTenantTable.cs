using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmentManager.Migrations
{
    /// <inheritdoc />
    public partial class DepositColumnTenantTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Deposit",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deposit",
                table: "Tenants");
        }
    }
}
