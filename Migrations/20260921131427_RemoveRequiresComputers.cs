using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequiresComputers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * Переносим старое требование RequiresComputers
             * в новую систему требований категорий аудиторий.
             *
             * Если предмету ранее требовались компьютеры,
             * добавляем ему категорию "Компьютерный класс".
             */

            migrationBuilder.Sql("""
                INSERT INTO "SubjectClassroomCategories"
                    ("SubjectId", "ClassroomCategoryId")
                SELECT
                    s."Id",
                    cc."Id"
                FROM "Subjects" s
                CROSS JOIN "ClassroomCategories" cc
                WHERE s."RequiresComputers" = TRUE
                  AND LOWER(TRIM(cc."Name")) = 'компьютерный класс'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "SubjectClassroomCategories" scr
                      WHERE scr."SubjectId" = s."Id"
                        AND scr."ClassroomCategoryId" = cc."Id"
                  );
                """);

            /*
             * После переноса данных старое поле больше не нужно.
             */
            migrationBuilder.DropColumn(
                name: "RequiresComputers",
                table: "Subjects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
             * Восстанавливаем старое поле.
             */
            migrationBuilder.AddColumn<bool>(
                name: "RequiresComputers",
                table: "Subjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            /*
             * Если предмет имеет требование категории
             * "Компьютерный класс", восстанавливаем
             * RequiresComputers = true.
             */
            migrationBuilder.Sql("""
                UPDATE "Subjects" s
                SET "RequiresComputers" = TRUE
                WHERE EXISTS (
                    SELECT 1
                    FROM "SubjectClassroomCategories" scr
                    INNER JOIN "ClassroomCategories" cc
                        ON cc."Id" = scr."ClassroomCategoryId"
                    WHERE scr."SubjectId" = s."Id"
                      AND LOWER(TRIM(cc."Name")) = 'компьютерный класс'
                );
                """);
        }
    }
}