using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JayamaliOptical.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddOpeningHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FriClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "FriClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FriOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HolClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HolClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HolOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MonClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MonClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MonOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SatClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SatClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SatOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SunClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SunClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SunOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThuClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ThuClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ThuOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TueClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TueClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TueOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WedClose",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "WedClosed",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WedOpen",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FriClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FriClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FriOpen",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HolClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HolClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "HolOpen",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "MonClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "MonClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "MonOpen",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SatClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SatClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SatOpen",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SunClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SunClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "SunOpen",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ThuClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ThuClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ThuOpen",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TueClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TueClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "TueOpen",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "WedClose",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "WedClosed",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "WedOpen",
                table: "SiteSettings");
        }
    }
}
