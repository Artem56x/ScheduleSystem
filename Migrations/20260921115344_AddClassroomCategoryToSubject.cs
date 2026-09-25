using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddClassroomCategoryToSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassroomCategoryId",
                table: "Subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ClassroomCategoryId",
                table: "Subjects",
                column: "ClassroomCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_ClassroomCategories_ClassroomCategoryId",
                table: "Subjects",
                column: "ClassroomCategoryId",
                principalTable: "ClassroomCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_ClassroomCategories_ClassroomCategoryId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_ClassroomCategoryId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ClassroomCategoryId",
                table: "Subjects");
        }
    }
}
