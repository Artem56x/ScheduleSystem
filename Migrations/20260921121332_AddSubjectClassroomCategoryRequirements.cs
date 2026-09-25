using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectClassroomCategoryRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "SubjectClassroomCategories",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    ClassroomCategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectClassroomCategories", x => new { x.SubjectId, x.ClassroomCategoryId });
                    table.ForeignKey(
                        name: "FK_SubjectClassroomCategories_ClassroomCategories_ClassroomCat~",
                        column: x => x.ClassroomCategoryId,
                        principalTable: "ClassroomCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectClassroomCategories_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectClassroomCategories_ClassroomCategoryId",
                table: "SubjectClassroomCategories",
                column: "ClassroomCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectClassroomCategories");

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
    }
}
