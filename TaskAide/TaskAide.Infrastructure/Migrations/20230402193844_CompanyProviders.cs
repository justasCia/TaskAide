using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Providers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompany",
                table: "Providers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WorkerId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_CompanyId",
                table: "Providers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_WorkerId",
                table: "Bookings",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Providers_WorkerId",
                table: "Bookings",
                column: "WorkerId",
                principalTable: "Providers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Providers_CompanyId",
                table: "Providers",
                column: "CompanyId",
                principalTable: "Providers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Providers_WorkerId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Providers_CompanyId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_CompanyId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_WorkerId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "IsCompany",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "WorkerId",
                table: "Bookings");
        }
    }
}
