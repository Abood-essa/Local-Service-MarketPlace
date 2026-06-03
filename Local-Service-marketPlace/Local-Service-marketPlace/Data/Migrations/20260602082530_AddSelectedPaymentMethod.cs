using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Local_Service_marketPlace.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedPaymentMethod",
                table: "Bookings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedPaymentMethod",
                table: "Bookings");
        }
    }
}
