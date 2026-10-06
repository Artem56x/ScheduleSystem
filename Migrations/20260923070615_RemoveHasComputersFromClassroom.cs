using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHasComputersFromClassroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasComputers",
                table: "Classrooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasComputers",
                table: "Classrooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
