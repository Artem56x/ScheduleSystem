using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Models;

namespace ScheduleSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Преподаватели
        public DbSet<Teacher> Teachers { get; set; }

        // Группы
        public DbSet<Group> Groups { get; set; }

        // Предметы
        public DbSet<Subject> Subjects { get; set; }

        // Аудитории
        public DbSet<Classroom> Classrooms { get; set; }

        // Расписание
        public DbSet<Schedule> Schedules { get; set; }

        // Категории аудиторий
        public DbSet<ClassroomCategory> ClassroomCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==============================
            // Schedule → Teacher
            // ==============================
            modelBuilder.Entity<Schedule>()
                .HasOne(schedule => schedule.Teacher)
                .WithMany()
                .HasForeignKey(schedule => schedule.TeacherId);

            // ==============================
            // Schedule → Group
            // ==============================
            modelBuilder.Entity<Schedule>()
                .HasOne(schedule => schedule.Group)
                .WithMany()
                .HasForeignKey(schedule => schedule.GroupId);

            // ==============================
            // Schedule → Subject
            // ==============================
            modelBuilder.Entity<Schedule>()
                .HasOne(schedule => schedule.Subject)
                .WithMany()
                .HasForeignKey(schedule => schedule.SubjectId);

            // ==============================
            // Teacher → Subject
            // ==============================
            modelBuilder.Entity<Teacher>()
                .HasOne(teacher => teacher.Subject)
                .WithMany()
                .HasForeignKey(teacher => teacher.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==============================
            // Schedule → Classroom
            // ==============================
            modelBuilder.Entity<Schedule>()
                .HasOne(schedule => schedule.Classroom)
                .WithMany()
                .HasForeignKey(schedule => schedule.ClassroomId);

            // ==============================
            // Classroom → ClassroomCategory
            // ==============================
            modelBuilder.Entity<Classroom>()
                .HasOne(classroom => classroom.ClassroomCategory)
                .WithMany(category => category.Classrooms)
                .HasForeignKey(classroom => classroom.ClassroomCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}