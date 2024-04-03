using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NBDProject2024.Data.NBDMigrations
{
    /// <inheritdoc />
    public partial class Approval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BidApproval",
                table: "Bids",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BidApproval",
                table: "Bids");
        }
    }
}
