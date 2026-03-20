using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JayamaliOptical.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<bool>(
            //    name: "RequiresPrescription",
            //    table: "Products",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrescriptionFileName",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescriptionId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescriptionImagePath",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PrescriptionId",
                table: "Orders",
                column: "PrescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Prescriptions_PrescriptionId",
                table: "Orders",
                column: "PrescriptionId",
                principalTable: "Prescriptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Prescriptions_PrescriptionId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PrescriptionId",
                table: "Orders");

            //migrationBuilder.DropColumn(
            //    name: "RequiresPrescription",
            //    table: "Products");

            migrationBuilder.DropColumn(
                name: "PrescriptionFileName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PrescriptionImagePath",
                table: "Orders");
        }
    }
}
