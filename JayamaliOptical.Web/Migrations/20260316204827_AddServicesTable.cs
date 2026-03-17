using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JayamaliOptical.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddServicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    RequiresAppointment = table.Column<bool>(type: "bit", nullable: false),
                    Button1Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button1Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button1Class = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button2Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button2Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button2Class = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button3Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button3Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Button3Class = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
