using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddClassroomCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Создаём таблицу категорий
            migrationBuilder.CreateTable(
                name: "ClassroomCategories",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "integer",
                        nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    Name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ClassroomCategories",
                        x => x.Id);
                });

            // 2. Добавляем временно nullable-ссылку
            migrationBuilder.AddColumn<int>(
                name: "ClassroomCategoryId",
                table: "Classrooms",
                type: "integer",
                nullable: true);

            // 3. Переносим старые категории из Classrooms.Category
            //
            // Для каждой уникальной категории создаётся запись
            // в ClassroomCategories.
            migrationBuilder.Sql("""
                INSERT INTO "ClassroomCategories" ("Name")
                SELECT DISTINCT
                    CASE
                        WHEN TRIM("Category") = '' THEN 'Без категории'
                        ELSE TRIM("Category")
                    END
                FROM "Classrooms"
                WHERE "Category" IS NOT NULL;
            """);

            // 4. Связываем существующие аудитории
            // с созданными категориями.
            migrationBuilder.Sql("""
                UPDATE "Classrooms" AS c
                SET "ClassroomCategoryId" = cc."Id"
                FROM "ClassroomCategories" AS cc
                WHERE cc."Name" =
                    CASE
                        WHEN TRIM(c."Category") = '' THEN 'Без категории'
                        ELSE TRIM(c."Category")
                    END;
            """);

            // 5. На случай пустой таблицы ClassroomCategories
            // создаём категорию по умолчанию.
            migrationBuilder.Sql("""
                INSERT INTO "ClassroomCategories" ("Name")
                SELECT 'Без категории'
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "ClassroomCategories"
                    WHERE "Name" = 'Без категории'
                );
            """);

            // 6. Заполняем возможные NULL значением
            // категории "Без категории".
            migrationBuilder.Sql("""
                UPDATE "Classrooms"
                SET "ClassroomCategoryId" = (
                    SELECT "Id"
                    FROM "ClassroomCategories"
                    WHERE "Name" = 'Без категории'
                    LIMIT 1
                )
                WHERE "ClassroomCategoryId" IS NULL;
            """);

            // 7. Теперь делаем поле обязательным
            migrationBuilder.AlterColumn<int>(
                name: "ClassroomCategoryId",
                table: "Classrooms",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // 8. Создаём индекс
            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_ClassroomCategoryId",
                table: "Classrooms",
                column: "ClassroomCategoryId");

            // 9. Создаём внешний ключ
            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_ClassroomCategories_ClassroomCategoryId",
                table: "Classrooms",
                column: "ClassroomCategoryId",
                principalTable: "ClassroomCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 10. Только теперь удаляем старую колонку Category
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Classrooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Возвращаем старую колонку Category
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Classrooms",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Восстанавливаем названия категорий
            migrationBuilder.Sql("""
                UPDATE "Classrooms" AS c
                SET "Category" = cc."Name"
                FROM "ClassroomCategories" AS cc
                WHERE c."ClassroomCategoryId" = cc."Id";
            """);

            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_ClassroomCategories_ClassroomCategoryId",
                table: "Classrooms");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_ClassroomCategoryId",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "ClassroomCategoryId",
                table: "Classrooms");

            migrationBuilder.DropTable(
                name: "ClassroomCategories");
        }
    }
}

