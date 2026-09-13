
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Controllers;

public class SchedulesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SchedulesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // INDEX
    // =========================================================

    public async Task<IActionResult> Index()
    {
        var schedules = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return View(schedules);
    }

    // =========================================================
    // DETAILS
    // =========================================================

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
            return NotFound();

        return View(schedule);
    }

    // =========================================================
    // CREATE - GET
    // =========================================================

    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();

        return View(new Schedule());
    }

    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Schedule schedule,
        bool confirmTeacherSubject = false)
    {
        RemoveDefaultValidationErrors();

        bool teacherSubjectMismatch =
            await ValidateScheduleAsync(
                schedule,
                confirmTeacherSubject);

        // -----------------------------------------------------
        // Предупреждение о соответствии преподавателя предмету
        // -----------------------------------------------------

        if (teacherSubjectMismatch && !confirmTeacherSubject)
        {
            await PopulateSelectListsAsync(schedule);

            return View(schedule);
        }

        // -----------------------------------------------------
        // Если есть ошибки — возвращаем форму
        // -----------------------------------------------------

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(schedule);

            return View(schedule);
        }

        // -----------------------------------------------------
        // Сохранение
        // -----------------------------------------------------

        _context.Schedules.Add(schedule);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // EDIT - GET
    // =========================================================

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var schedule = await _context.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id.Value);

        if (schedule == null)
            return NotFound();

        await PopulateSelectListsAsync(schedule);

        return View(schedule);
    }

    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Schedule schedule,
        bool confirmTeacherSubject = false)
    {
        if (id != schedule.Id)
            return NotFound();

        RemoveDefaultValidationErrors();

        bool teacherSubjectMismatch =
            await ValidateScheduleAsync(
                schedule,
                confirmTeacherSubject);

        // -----------------------------------------------------
        // Предупреждение преподаватель → предмет
        // -----------------------------------------------------

        if (teacherSubjectMismatch && !confirmTeacherSubject)
        {
            await PopulateSelectListsAsync(schedule);

            return View(schedule);
        }

        // -----------------------------------------------------
        // Ошибки валидации
        // -----------------------------------------------------

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(schedule);

            return View(schedule);
        }

        // -----------------------------------------------------
        // Обновление
        // -----------------------------------------------------

        try
        {
            _context.Schedules.Update(schedule);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ScheduleExistsAsync(schedule.Id))
                return NotFound();

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // DELETE - GET
    // =========================================================

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
            return NotFound();

        return View(schedule);
    }

    // =========================================================
    // DELETE - POST
    // =========================================================

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var schedule = await _context.Schedules
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule != null)
        {
            _context.Schedules.Remove(schedule);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // ПОЛУЧЕНИЕ РАСПИСАНИЯ
    // =========================================================

    private async Task<Schedule?> FindScheduleAsync(int id)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // =========================================================
    // SELECT LISTS
    // =========================================================

    private async Task PopulateSelectListsAsync(
        Schedule? schedule = null)
    {
        var teachers = await _context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync();

        var groups = await _context.Groups
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync();

        var subjects = await _context.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();

        var classrooms = await _context.Classrooms
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewData["TeacherId"] = new SelectList(
            teachers,
            "Id",
            "FullName",
            schedule?.TeacherId);

        ViewData["GroupId"] = new SelectList(
            groups,
            "Id",
            "Name",
            schedule?.GroupId);

        ViewData["SubjectId"] = new SelectList(
            subjects,
            "Id",
            "Name",
            schedule?.SubjectId);

        ViewData["ClassroomId"] = new SelectList(
            classrooms,
            "Id",
            "Name",
            schedule?.ClassroomId);

        var dayNames = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "Понедельник" },
            { DayOfWeek.Tuesday, "Вторник" },
            { DayOfWeek.Wednesday, "Среда" },
            { DayOfWeek.Thursday, "Четверг" },
            { DayOfWeek.Friday, "Пятница" },
            { DayOfWeek.Saturday, "Суббота" },
            { DayOfWeek.Sunday, "Воскресенье" }
        };

        ViewData["DayOfWeek"] = new SelectList(
            dayNames.Select(x => new
            {
                Value = (int)x.Key,
                Text = x.Value
            }),
            "Value",
            "Text",
            schedule?.DayOfWeek.HasValue == true
                ? (int)schedule.DayOfWeek.Value
                : null);
    }

    // =========================================================
    // УДАЛЕНИЕ СТАНДАРТНЫХ ОШИБОК MVC
    // =========================================================

    private void RemoveDefaultValidationErrors()
    {
        var keysToRemove = new[]
        {
            nameof(Schedule.TeacherId),
            nameof(Schedule.GroupId),
            nameof(Schedule.SubjectId),
            nameof(Schedule.ClassroomId),
            nameof(Schedule.DayOfWeek),
            nameof(Schedule.StartTime),
            nameof(Schedule.EndTime)
        };

        foreach (var key in keysToRemove)
        {
            if (ModelState.ContainsKey(key))
            {
                ModelState[key]!.Errors.Clear();
            }
        }
    }

    // =========================================================
    // ГЛАВНАЯ ПРОВЕРКА РАСПИСАНИЯ
    // =========================================================

    private async Task<bool> ValidateScheduleAsync(
        Schedule schedule,
        bool confirmTeacherSubject = false)
    {
        // =====================================================
        // 1. БАЗОВАЯ ВАЛИДАЦИЯ
        // =====================================================

        bool basicDataValid = ValidateBasicData(schedule);

        if (!basicDataValid)
            return false;

        // =====================================================
        // 2. ЗАГРУЗКА СВЯЗАННЫХ ОБЪЕКТОВ
        // =====================================================

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Id == schedule.TeacherId);

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g =>
                g.Id == schedule.GroupId);

        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == schedule.SubjectId);

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == schedule.ClassroomId);

        // =====================================================
        // 3. ПРОВЕРКА СУЩЕСТВОВАНИЯ
        // =====================================================

        if (teacher == null)
        {
            ModelState.AddModelError(
                nameof(schedule.TeacherId),
                "Выбранный преподаватель не найден.");
        }

        if (group == null)
        {
            ModelState.AddModelError(
                nameof(schedule.GroupId),
                "Выбранная группа не найдена.");
        }

        if (subject == null)
        {
            ModelState.AddModelError(
                nameof(schedule.SubjectId),
                "Выбранный предмет не найден.");
        }

        if (classroom == null)
        {
            ModelState.AddModelError(
                nameof(schedule.ClassroomId),
                "Выбранная аудитория не найдена.");
        }

        if (teacher == null ||
            group == null ||
            subject == null ||
            classroom == null)
        {
            return false;
        }

        // =====================================================
        // 4. ПРЕДМЕТ ПРЕПОДАВАТЕЛЯ
        // =====================================================

        bool teacherSubjectMismatch =
            teacher.SubjectId != subject.Id;

        if (teacherSubjectMismatch && !confirmTeacherSubject)
        {
            await SetTeacherSubjectWarningAsync(
                teacher,
                subject);
        }

        // =====================================================
        // 5. ВМЕСТИМОСТЬ АУДИТОРИИ
        // =====================================================

        bool capacityError =
            group.StudentCount > classroom.Capacity;

        if (capacityError)
        {
            ModelState.AddModelError(
                nameof(schedule.ClassroomId),
                $"В аудитории «{classroom.Name}» недостаточно мест. " +
                $"Вместимость: {classroom.Capacity}, " +
                $"в группе: {group.StudentCount} человек.");
        }

        // =====================================================
        // 6. КОМПЬЮТЕРЫ
        // =====================================================

        bool computerError =
            subject.RequiresComputers &&
            !classroom.HasComputers;

        if (computerError)
        {
            ModelState.AddModelError(
                nameof(schedule.ClassroomId),
                $"Для предмета «{subject.Name}» нужна " +
                $"аудитория с компьютерами.");
        }

        // =====================================================
        // 7. КОНФЛИКТ АУДИТОРИИ
        // =====================================================

        bool classroomConflict =
            await HasClassroomConflictAsync(schedule);

        if (classroomConflict)
        {
            ModelState.AddModelError(
                nameof(schedule.ClassroomId),
                $"Аудитория «{classroom.Name}» уже занята " +
                $"в выбранный день и время.");
        }

        // =====================================================
        // 8. КОНФЛИКТ ГРУППЫ
        // =====================================================

        bool groupConflict =
            await HasGroupConflictAsync(schedule);

        if (groupConflict)
        {
            ModelState.AddModelError(
                nameof(schedule.GroupId),
                $"Группа «{group.Name}» уже имеет занятие " +
                $"в выбранный день и время.");
        }

        // =====================================================
        // 9. КОНФЛИКТ ПРЕПОДАВАТЕЛЯ
        // =====================================================

        bool teacherConflict =
            await HasTeacherConflictAsync(schedule);

        if (teacherConflict)
        {
            ModelState.AddModelError(
                nameof(schedule.TeacherId),
                $"Преподаватель «{teacher.FullName}» " +
                $"уже занят в выбранный день и время.");
        }

        // =====================================================
        // 10. РЕКОМЕНДАЦИИ АУДИТОРИЙ
        // =====================================================

        if (classroomConflict ||
            capacityError ||
            computerError)
        {
            await BuildClassroomRecommendationsAsync(
                schedule,
                group,
                subject,
                classroom);
        }

        return teacherSubjectMismatch;
    }

    // =========================================================
    // БАЗОВАЯ ПРОВЕРКА ДАННЫХ
    // =========================================================

    private bool ValidateBasicData(Schedule schedule)
    {
        bool valid = true;

        if (schedule.TeacherId <= 0)
        {
            ModelState.AddModelError(
                nameof(schedule.TeacherId),
                "Выберите преподавателя.");

            valid = false;
        }

        if (schedule.GroupId <= 0)
        {
            ModelState.AddModelError(
                nameof(schedule.GroupId),
                "Выберите группу.");

            valid = false;
        }

        if (schedule.SubjectId <= 0)
        {
            ModelState.AddModelError(
                nameof(schedule.SubjectId),
                "Выберите предмет.");

            valid = false;
        }

        if (schedule.ClassroomId <= 0)
        {
            ModelState.AddModelError(
                nameof(schedule.ClassroomId),
                "Выберите аудиторию.");

            valid = false;
        }

        if (!schedule.DayOfWeek.HasValue)
        {
            ModelState.AddModelError(
                nameof(schedule.DayOfWeek),
                "Выберите день недели.");

            valid = false;
        }

        if (!schedule.StartTime.HasValue)
        {
            ModelState.AddModelError(
                nameof(schedule.StartTime),
                "Укажите время начала.");

            valid = false;
        }

        if (!schedule.EndTime.HasValue)
        {
            ModelState.AddModelError(
                nameof(schedule.EndTime),
                "Укажите время окончания.");

            valid = false;
        }

        if (schedule.StartTime.HasValue &&
            schedule.EndTime.HasValue &&
            schedule.EndTime.Value <= schedule.StartTime.Value)
        {
            ModelState.AddModelError(
                nameof(schedule.EndTime),
                "Время окончания должно быть позже времени начала.");

            valid = false;
        }

        return valid;
    }

    // =========================================================
    // КОНФЛИКТ АУДИТОРИИ
    // =========================================================

    private async Task<bool> HasClassroomConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules.AnyAsync(s =>
            s.Id != schedule.Id &&
            s.ClassroomId == schedule.ClassroomId &&
            s.DayOfWeek == schedule.DayOfWeek &&
            s.StartTime < schedule.EndTime &&
            s.EndTime > schedule.StartTime);
    }

    // =========================================================
    // КОНФЛИКТ ГРУППЫ
    // =========================================================

    private async Task<bool> HasGroupConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules.AnyAsync(s =>
            s.Id != schedule.Id &&
            s.GroupId == schedule.GroupId &&
            s.DayOfWeek == schedule.DayOfWeek &&
            s.StartTime < schedule.EndTime &&
            s.EndTime > schedule.StartTime);
    }

    // =========================================================
    // КОНФЛИКТ ПРЕПОДАВАТЕЛЯ
    // =========================================================

    private async Task<bool> HasTeacherConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules.AnyAsync(s =>
            s.Id != schedule.Id &&
            s.TeacherId == schedule.TeacherId &&
            s.DayOfWeek == schedule.DayOfWeek &&
            s.StartTime < schedule.EndTime &&
            s.EndTime > schedule.StartTime);
    }

    // =========================================================
    // РЕКОМЕНДАЦИИ АУДИТОРИЙ
    // =========================================================

    private async Task BuildClassroomRecommendationsAsync(
        Schedule schedule,
        Group group,
        Subject subject,
        Classroom selectedClassroom)
    {
        // -----------------------------------------------------
        // Получаем аудитории, занятые в это время.
        // -----------------------------------------------------

        var busyClassroomIds = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.Id != schedule.Id &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime)
            .Select(s => s.ClassroomId)
            .Distinct()
            .ToListAsync();

        // -----------------------------------------------------
        // Находим подходящие свободные аудитории.
        // -----------------------------------------------------

        var recommendedClassrooms =
            await _context.Classrooms
                .AsNoTracking()
                .Where(c =>
                    !busyClassroomIds.Contains(c.Id) &&
                    c.Capacity >= group.StudentCount &&
                    (!subject.RequiresComputers ||
                     c.HasComputers))
                .OrderBy(c => c.Capacity)
                .ThenBy(c => c.Name)
                .ToListAsync();

        ViewBag.RecommendedClassrooms =
            recommendedClassrooms;

        // -----------------------------------------------------
        // Данные для отображения в представлении.
        // -----------------------------------------------------

        ViewBag.RequiredStudentCount =
            group.StudentCount;

        ViewBag.SelectedClassroomName =
            selectedClassroom.Name;

        ViewBag.SelectedClassroomCapacity =
            selectedClassroom.Capacity;

        ViewBag.RequiredComputers =
            subject.RequiresComputers;

        ViewBag.RecommendedClassroomRemainingSeats =
            recommendedClassrooms.ToDictionary(
                c => c.Id,
                c => c.Capacity - group.StudentCount);
    }

    // =========================================================
    // ПРЕДУПРЕЖДЕНИЕ ПРЕДМЕТА ПРЕПОДАВАТЕЛЯ
    // =========================================================

    private async Task SetTeacherSubjectWarningAsync(
        Teacher teacher,
        Subject selectedSubject)
    {
        ViewBag.TeacherSubjectWarning = true;

        ViewBag.TeacherName =
            teacher.FullName;

        ViewBag.TeacherSubjectName =
            teacher.SubjectId.HasValue
                ? await _context.Subjects
                    .AsNoTracking()
                    .Where(s => s.Id == teacher.SubjectId.Value)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync()
                    ?? "другой предмет"
                : "не указан";

        ViewBag.SelectedSubjectName =
            selectedSubject.Name;
    }

    // =========================================================
    // ПРОВЕРКА СУЩЕСТВОВАНИЯ
    // =========================================================

    private async Task<bool> ScheduleExistsAsync(int id)
    {
        return await _context.Schedules
            .AnyAsync(e => e.Id == id);
    }
}

