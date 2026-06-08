using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NBDProject2024.Data.NBDMigrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueWorkOrderPerDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProjectID_ScheduledDate",
                table: "WorkOrders");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProjectID_ScheduledDate",
                table: "WorkOrders",
                columns: new[] { "ProjectID", "ScheduledDate" },
                unique: true,
                filter: "Status <> 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProjectID_ScheduledDate",
                table: "WorkOrders");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProjectID_ScheduledDate",
                table: "WorkOrders",
                columns: new[] { "ProjectID", "ScheduledDate" });
        }
    }
}
