using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JayamaliOptical.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RightEyeSphere = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RightEyeCylinder = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RightEyeAxis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RightEyeAdd = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LeftEyeSphere = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LeftEyeCylinder = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LeftEyeAxis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LeftEyeAdd = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PupillaryDistance = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileMimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UserId",
                table: "Prescriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prescriptions");
        }
    }
}
