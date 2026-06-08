using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NBDProject2024.Data.NBDMigrations
{
    /// <inheritdoc />
    public partial class AddPurchaseApprovalAndKardex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "PurchaseRequests",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "PurchaseRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MovementDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MovementType = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialID = table.Column<int>(type: "INTEGER", nullable: false),
                    StockLocationID = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityDelta = table.Column<double>(type: "REAL", nullable: false),
                    QuantityBefore = table.Column<double>(type: "REAL", nullable: false),
                    QuantityAfter = table.Column<double>(type: "REAL", nullable: false),
                    UnitCost = table.Column<double>(type: "REAL", nullable: false),
                    ReferenceCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Materials_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Materials",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_StockLocations_StockLocationID",
                        column: x => x.StockLocationID,
                        principalTable: "StockLocations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_MaterialID",
                table: "InventoryMovements",
                column: "MaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_StockLocationID_MaterialID_MovementDate",
                table: "InventoryMovements",
                columns: new[] { "StockLocationID", "MaterialID", "MovementDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "PurchaseRequests");
        }
    }
}
