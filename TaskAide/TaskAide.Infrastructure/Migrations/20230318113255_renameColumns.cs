using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace TaskAide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class renameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DefaultPricePerHour",
                table: "Providers",
                newName: "BasePricePerHour");

            migrationBuilder.AlterColumn<Point>(
                name: "Address",
                table: "Bookings",
                type: "geography",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BasePricePerHour",
                table: "Providers",
                newName: "DefaultPricePerHour");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geography");
        }
    }
}
