using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BookingNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClientSeen",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProviderSeen",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientSeen",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ProviderSeen",
                table: "Bookings");
        }
    }
}
