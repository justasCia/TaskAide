using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BookingPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutId",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Paid",
                table: "Bookings");
        }
    }
}
