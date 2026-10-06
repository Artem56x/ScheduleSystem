using System.Collections.Generic;

namespace ScheduleSystem.ViewModels;

public class AnalyticsViewModel
{
    // ============================================================
    // ОБЩАЯ СТАТИСТИКА
    // ============================================================

    public int TeachersCount { get; set; }
    public int GroupsCount { get; set; }
    public int SubjectsCount { get; set; }
    public int ClassroomsCount { get; set; }
    public int SchedulesCount { get; set; }


    // ============================================================
    // ЗАНЯТИЯ ПО ДНЯМ НЕДЕЛИ
    // ============================================================

    public List<DayAnalyticsItem> LessonsByDay { get; set; } = new();


    // ============================================================
    // ЗАНЯТИЯ ПО ВРЕМЕНИ НАЧАЛА
    // ============================================================

    public List<TimeAnalyticsItem> LessonsByTime { get; set; } = new();


    // ============================================================
    // НАГРУЗКА ПРЕПОДАВАТЕЛЕЙ
    // ============================================================

    public List<TeacherAnalyticsItem> TeacherLoad { get; set; } = new();


    // ============================================================
    // НАГРУЗКА ГРУПП
    // ============================================================

    public List<GroupAnalyticsItem> GroupLoad { get; set; } = new();


    // ============================================================
    // ИСПОЛЬЗОВАНИЕ АУДИТОРИЙ
    // ============================================================

    public List<ClassroomAnalyticsItem> ClassroomUsage { get; set; } = new();


    // ============================================================
    // ИСПОЛЬЗОВАНИЕ ПРЕДМЕТОВ
    // ============================================================

    public List<SubjectAnalyticsItem> SubjectUsage { get; set; } = new();


    // ============================================================
    // ДОПОЛНИТЕЛЬНЫЕ ПОКАЗАТЕЛИ
    // ============================================================

    public double AverageLessonsPerDay { get; set; }

    public double AverageLessonsPerTeacher { get; set; }

    public double AverageLessonsPerGroup { get; set; }

    public string BusiestDay { get; set; } = "—";

    public int BusiestDayCount { get; set; }

    public string BusiestTime { get; set; } = "—";

    public int BusiestTimeCount { get; set; }
}


// ================================================================
// ДЕНЬ НЕДЕЛИ
// ================================================================

public class DayAnalyticsItem
{
    public string DayName { get; set; } = "";

    public int Count { get; set; }
}


// ================================================================
// ВРЕМЯ
// ================================================================

public class TimeAnalyticsItem
{
    public string Time { get; set; } = "";

    public int Count { get; set; }
}


// ================================================================
// ПРЕПОДАВАТЕЛЬ
// ================================================================

public class TeacherAnalyticsItem
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int LessonsCount { get; set; }
}


// ================================================================
// ГРУППА
// ================================================================

public class GroupAnalyticsItem
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int LessonsCount { get; set; }
}


// ================================================================
// АУДИТОРИЯ
// ================================================================

public class ClassroomAnalyticsItem
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int LessonsCount { get; set; }

    public int Capacity { get; set; }
}


// ================================================================
// ПРЕДМЕТ
// ================================================================

public class SubjectAnalyticsItem
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int LessonsCount { get; set; }
}