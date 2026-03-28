using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JayamaliOptical.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone3 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FacebookUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    WhatsAppUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TikTokUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    InstagramUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PrivacyPolicy = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    History = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Vision = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Mission = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings");
        }
    }
}
