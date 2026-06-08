using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NBDProject2024.Data.NBDMigrations
{
    /// <inheritdoc />
    public partial class AddCrewSkillsAndRoutePlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeSkills",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false),
                    Skill = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSkills", x => new { x.EmployeeID, x.Skill });
                    table.ForeignKey(
                        name: "FK_EmployeeSkills_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderCrewAssignments",
                columns: table => new
                {
                    WorkOrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedSkill = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedHours = table.Column<double>(type: "REAL", nullable: false),
                    ActualHours = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderCrewAssignments", x => new { x.WorkOrderID, x.EmployeeID });
                    table.ForeignKey(
                        name: "FK_WorkOrderCrewAssignments_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderCrewAssignments_WorkOrders_WorkOrderID",
                        column: x => x.WorkOrderID,
                        principalTable: "WorkOrders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCrewAssignments_AssignedSkill_WorkOrderID",
                table: "WorkOrderCrewAssignments",
                columns: new[] { "AssignedSkill", "WorkOrderID" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCrewAssignments_EmployeeID",
                table: "WorkOrderCrewAssignments",
                column: "EmployeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSkills");

            migrationBuilder.DropTable(
                name: "WorkOrderCrewAssignments");
        }
    }
}
