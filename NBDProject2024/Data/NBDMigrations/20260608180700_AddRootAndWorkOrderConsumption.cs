using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NBDProject2024.Data.NBDMigrations
{
    /// <inheritdoc />
    public partial class AddRootAndWorkOrderConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderMaterialConsumptions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkOrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialID = table.Column<int>(type: "INTEGER", nullable: false),
                    StockLocationID = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    QuantityUsed = table.Column<double>(type: "REAL", nullable: false),
                    UnitCostAtUse = table.Column<double>(type: "REAL", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderMaterialConsumptions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WorkOrderMaterialConsumptions_Materials_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Materials",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderMaterialConsumptions_StockLocations_StockLocationID",
                        column: x => x.StockLocationID,
                        principalTable: "StockLocations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderMaterialConsumptions_WorkOrders_WorkOrderID",
                        column: x => x.WorkOrderID,
                        principalTable: "WorkOrders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderMaterialConsumptions_MaterialID",
                table: "WorkOrderMaterialConsumptions",
                column: "MaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderMaterialConsumptions_StockLocationID",
                table: "WorkOrderMaterialConsumptions",
                column: "StockLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderMaterialConsumptions_WorkOrderID_ConsumedOn",
                table: "WorkOrderMaterialConsumptions",
                columns: new[] { "WorkOrderID", "ConsumedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderMaterialConsumptions");
        }
    }
}
