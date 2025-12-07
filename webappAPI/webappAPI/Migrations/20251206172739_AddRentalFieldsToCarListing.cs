using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webappAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalFieldsToCarListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyRentalRate",
                table: "CarListings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForRental",
                table: "CarListings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RentalStock",
                table: "CarListings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyRentalRate",
                table: "CarListings");

            migrationBuilder.DropColumn(
                name: "IsAvailableForRental",
                table: "CarListings");

            migrationBuilder.DropColumn(
                name: "RentalStock",
                table: "CarListings");
        }
    }
}
