using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectTypesAndGroupSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Subjects");
        }
    }
}
