using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JayamaliOptical.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixCodEnabledDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The AddPaymentFields migration defaulted this column to false, contradicting
            // the C# model's default of true (SiteSettings.CodEnabled = true). Correct the
            // column-level default so any future row inserted without an explicit value
            // matches the model. This does not touch existing row data — see
            // DbInitializer's narrow self-heal for that.
            migrationBuilder.AlterColumn<bool>(
                name: "CodEnabled",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "CodEnabled",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}
