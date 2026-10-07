using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================
            // 1. Создаём таблицу связи Teacher ↔ Subject
            // =========================================================

            migrationBuilder.CreateTable(
                name: "TeacherSubjects",
                columns: table => new
                {
                    TeacherId = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    SubjectId = table.Column<int>(
                        type: "integer",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_TeacherSubjects",
                        x => new { x.TeacherId, x.SubjectId });

                    table.ForeignKey(
                        name: "FK_TeacherSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_TeacherSubjects_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_SubjectId",
                table: "TeacherSubjects",
                column: "SubjectId");


            // =========================================================
            // 2. Переносим существующие назначения
            //
            // Старое:
            // Teachers.SubjectId
            //
            // Новое:
            // TeacherSubjects.TeacherId + SubjectId
            // =========================================================

            migrationBuilder.Sql("""
                INSERT INTO "TeacherSubjects" ("TeacherId", "SubjectId")
                SELECT "Id", "SubjectId"
                FROM "Teachers"
                WHERE "SubjectId" IS NOT NULL;
                """);


            // =========================================================
            // 3. Удаляем старую связь Teachers → Subjects
            // =========================================================

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Subjects_SubjectId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_SubjectId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Teachers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =========================================================
            // 1. Возвращаем старое поле
            // =========================================================

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "Teachers",
                type: "integer",
                nullable: true);


            // =========================================================
            // 2. Восстанавливаем первое назначение предмета
            //
            // Старый формат позволял только один SubjectId,
            // поэтому при откате берём минимальный SubjectId
            // для каждого преподавателя.
            // =========================================================

            migrationBuilder.Sql("""
                UPDATE "Teachers" AS t
                SET "SubjectId" = ts."SubjectId"
                FROM (
                    SELECT DISTINCT ON ("TeacherId")
                        "TeacherId",
                        "SubjectId"
                    FROM "TeacherSubjects"
                    ORDER BY "TeacherId", "SubjectId"
                ) AS ts
                WHERE t."Id" = ts."TeacherId";
                """);


            // =========================================================
            // 3. Восстанавливаем индекс и FK
            // =========================================================

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_SubjectId",
                table: "Teachers",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Subjects_SubjectId",
                table: "Teachers",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");


            // =========================================================
            // 4. Удаляем новую таблицу
            // =========================================================

            migrationBuilder.DropTable(
                name: "TeacherSubjects");
        }
    }
}