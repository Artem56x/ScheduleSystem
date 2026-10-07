using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Models;

namespace ScheduleSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Преподаватели
        public DbSet<Teacher> Teachers { get; set; }

        // Связь Преподаватель ↔ Предмет
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }

        // Группы
        public DbSet<Group> Groups { get; set; }

        // Предметы
        public DbSet<Subject> Subjects { get; set; }

        // Аудитории
        public DbSet<Classroom> Classrooms { get; set; }

        // Расписание
        public DbSet<Schedule> Schedules { get; set; }

        public DbSet<ScheduleTimeSlot> ScheduleTimeSlots { get; set; }

        public DbSet<ScheduleGenerationSettings> ScheduleGenerationSettings { get; set; }

        // Категории аудиторий
        public DbSet<ClassroomCategory> ClassroomCategories { get; set; }

        // Уведомления
        public DbSet<Notification> Notifications { get; set; }

        // Журнал
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<GroupSubject> GroupSubjects { get; set; }

        public DbSet<SubjectClassroomCategory> SubjectClassroomCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==============================
            // Teacher ↔ Subject
            // ==============================

            modelBuilder.Entity<TeacherSubject>()
                .HasKey(x => new
                {
                    x.TeacherId,
                    x.SubjectId
                });

            modelBuilder.Entity<TeacherSubject>()
                .HasOne(x => x.Teacher)
                .WithMany(x => x.TeacherSubjects)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherSubject>()
                .HasOne(x => x.Subject)
                .WithMany(x => x.TeacherSubjects)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==============================
            // Subject ↔ ClassroomCategory
            // ==============================

            modelBuilder.Entity<SubjectClassroomCategory>()
                .HasKey(x => new
                {
                    x.SubjectId,
                    x.ClassroomCategoryId
                });

            modelBuilder.Entity<SubjectClassroomCategory>()
                .HasOne(x => x.Subject)
                .WithMany(x => x.ClassroomCategoryRequirements)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubjectClassroomCategory>()
                .HasOne(x => x.ClassroomCategory)
                .WithMany(x => x.SubjectRequirements)
                .HasForeignKey(x => x.ClassroomCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

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