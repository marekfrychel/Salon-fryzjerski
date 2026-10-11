using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonFryzjerski.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Availabilities",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "Availabilities",
                newName: "EndTime");

            migrationBuilder.AddColumn<bool>(
                name: "IsWorking",
                table: "Availabilities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWorking",
                table: "Availabilities");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Availabilities",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Availabilities",
                newName: "End");
        }
    }
}
