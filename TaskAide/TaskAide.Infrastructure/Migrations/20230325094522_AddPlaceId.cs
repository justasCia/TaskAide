using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaceId",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlaceId",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "Bookings");
        }
    }
}
