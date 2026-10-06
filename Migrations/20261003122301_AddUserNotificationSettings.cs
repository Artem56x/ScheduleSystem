using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ScheduleNotificationsEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SystemNotificationsEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleNotificationsEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SystemNotificationsEnabled",
                table: "AspNetUsers");
        }
    }
}
