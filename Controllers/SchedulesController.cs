using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
using ScheduleSystem.Services;

namespace ScheduleSystem.Controllers;

public class SchedulesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ScheduleExportService _exportService;

    public SchedulesController(
        ApplicationDbContext context,
        ScheduleExportService exportService)
    {
        _context = context;
        _exportService = exportService;
    }


    // ============================================================
    // INDEX
    // ============================================================

    public async Task<IActionResult> Index(
        string? group,
        string? teacher,
        string? classroom,
        string? subject,
        DayOfWeek? dayOfWeek,
        TimeSpan? timeFrom,
        TimeSpan? timeTo)
    {
        var query = _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(group))
        {
            query = query.Where(s =>
                s.Group != null &&
                s.Group.Name.Contains(group));
        }

        if (!string.IsNullOrWhiteSpace(teacher))
        {
            query = query.Where(s =>
                s.Teacher != null &&
                s.Teacher.FullName.Contains(teacher));
        }

        if (!string.IsNullOrWhiteSpace(classroom))
        {
            query = query.Where(s =>
                s.Classroom != null &&
                s.Classroom.Name.Contains(classroom));
        }

        if (!string.IsNullOrWhiteSpace(subject))
        {
            query = query.Where(s =>
                s.Subject != null &&
                s.Subject.Name.Contains(subject));
        }

        if (dayOfWeek.HasValue)
        {
            query = query.Where(s =>
                s.DayOfWeek == dayOfWeek);
        }

        if (timeFrom.HasValue)
        {
            query = query.Where(s =>
                s.StartTime >= timeFrom);
        }

        if (timeTo.HasValue)
        {
            query = query.Where(s =>
                s.EndTime <= timeTo);
        }

        var schedules = await query
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        ViewBag.Groups = await _context.Groups
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync();

        ViewBag.Teachers = await _context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync();

        ViewBag.Classrooms = await _context.Classrooms
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Subjects = await _context.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.SelectedGroup = group;
        ViewBag.SelectedTeacher = teacher;
        ViewBag.SelectedClassroom = classroom;
        ViewBag.SelectedSubject = subject;
        ViewBag.SelectedDayOfWeek = dayOfWeek;
        ViewBag.SelectedTimeFrom = timeFrom;
        ViewBag.SelectedTimeTo = timeTo;

        ViewBag.TotalCount = schedules.Count;

        ViewBag.SubjectCount = schedules
            .Select(s => s.SubjectId)
            .Distinct()
            .Count();

        ViewBag.GroupCount = schedules
            .Select(s => s.GroupId)
            .Distinct()
            .Count();

        ViewBag.TeacherCount = schedules
            .Select(s => s.TeacherId)
            .Distinct()
            .Count();

        ViewBag.ClassroomCount = schedules
            .Select(s => s.ClassroomId)
            .Distinct()
            .Count();

        return View(schedules);
    }

    // ============================================================
    // GROUP SCHEDULE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GroupSchedule(int? id)
    {
        var groups = await _context.Groups
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync();

        ViewBag.Groups = groups;

        if (id == null)
        {
            return View(new List<Schedule>());
        }

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        ViewBag.SelectedGroupId = group.Id;
        ViewBag.SelectedGroupName = group.Name;

        var schedules = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .Where(s => s.GroupId == id)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return View(schedules);
    }


    // ============================================================
    // TEACHER SCHEDULE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> TeacherSchedule(int? id)
    {
        var teachers = await _context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync();

        ViewBag.Teachers = teachers;

        if (id == null)
        {
            return View(new List<Schedule>());
        }

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound();

        ViewBag.SelectedTeacherId = teacher.Id;
        ViewBag.SelectedTeacherName = teacher.FullName;

        var schedules = await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .Where(s => s.TeacherId == id)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return View(schedules);
    }

    // ============================================================
    // EXPORT PAGE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var groups = await _context.Groups
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync();

        var teachers = await _context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync();

        ViewBag.Teachers = teachers;

        return View(groups);
    }


    // ============================================================
    // EXCEL
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> ExportAllExcel()
    {
        var schedules = await GetAllSchedulesForExportAsync();

        var fileBytes =
            _exportService.CreateAllExcel(schedules);

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Общее_расписание.xlsx");
    }


    [HttpGet]
    public async Task<IActionResult> ExportGroupExcel(int id)
    {
        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        var schedules = await GetGroupSchedulesForExportAsync(id);

        var fileBytes =
            _exportService.CreateGroupExcel(
                group.Name,
                schedules);

        var fileName =
            $"Расписание_{MakeSafeFileName(group.Name)}.xlsx";

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }


    [HttpGet]
    public async Task<IActionResult> ExportTeacherExcel(int id)
    {
        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound();

        var schedules =
            await GetTeacherSchedulesForExportAsync(id);

        var fileBytes =
            _exportService.CreateTeacherExcel(
                teacher.FullName,
                schedules);

        var fileName =
            $"Расписание_{MakeSafeFileName(teacher.FullName)}.xlsx";

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }


    // ============================================================
    // PDF
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> ExportAllPdf()
    {
        var schedules =
            await GetAllSchedulesForExportAsync();

        var fileBytes =
            _exportService.CreateAllPdf(schedules);

        return File(
            fileBytes,
            "application/pdf",
            "Общее_расписание.pdf");
    }


    [HttpGet]
    public async Task<IActionResult> ExportGroupPdf(int id)
    {
        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        var schedules =
            await GetGroupSchedulesForExportAsync(id);

        var fileBytes =
            _exportService.CreateGroupPdf(
                group.Name,
                schedules);

        var fileName =
            $"Расписание_{MakeSafeFileName(group.Name)}.pdf";

        return File(
            fileBytes,
            "application/pdf",
            fileName);
    }


    [HttpGet]
    public async Task<IActionResult> ExportTeacherPdf(int id)
    {
        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound();

        var schedules =
            await GetTeacherSchedulesForExportAsync(id);

        var fileBytes =
            _exportService.CreateTeacherPdf(
                teacher.FullName,
                schedules);

        var fileName =
            $"Расписание_{MakeSafeFileName(teacher.FullName)}.pdf";

        return File(
            fileBytes,
            "application/pdf",
            fileName);
    }


    // ============================================================
    // CSV
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> ExportAllCsv()
    {
        var schedules =
            await GetAllSchedulesForExportAsync();

        var fileBytes =
            _exportService.CreateAllCsv(schedules);

        return File(
            fileBytes,
            "text/csv; charset=utf-8",
            "Общее_расписание.csv");
    }


    [HttpGet]
    public async Task<IActionResult> ExportGroupCsv(int id)
    {
        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        var schedules =
            await GetGroupSchedulesForExportAsync(id);

        var fileBytes =
            _exportService.CreateGroupCsv(
                group.Name,
                schedules);

        var fileName =
            $"Расписание_{MakeSafeFileName(group.Name)}.csv";

        return File(
            fileBytes,
            "text/csv; charset=utf-8",
            fileName);
    }


    [HttpGet]
    public async Task<IActionResult> ExportTeacherCsv(int id)
    {
        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound();

        var schedules =
            await GetTeacherSchedulesForExportAsync(id);

        var fileBytes =
            _exportService.CreateTeacherCsv(
                teacher.FullName,
                schedules);

        var fileName =
            $"Расписание_{MakeSafeFileName(teacher.FullName)}.csv";

        return File(
            fileBytes,
            "text/csv; charset=utf-8",
            fileName);
    }


    // ============================================================
    // DETAILS
    // ============================================================

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
            return NotFound();

        return View(schedule);
    }


    // ============================================================
    // CREATE
    // ============================================================

    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Schedule schedule,
        bool confirmTeacherSubject = false)
    {
        RemoveDefaultValidationErrors();

        var validationResult =
            await ValidateScheduleAsync(
                schedule,
                confirmTeacherSubject);

        if (!validationResult.IsValid)
        {
            await PopulateSelectListsAsync(schedule);
            return View(schedule);
        }

        await SetTeacherSubjectWarningAsync(schedule);

        if (ViewBag.TeacherSubjectWarning == true &&
            !confirmTeacherSubject)
        {
            await PopulateSelectListsAsync(schedule);
            return View(schedule);
        }

        _context.Schedules.Add(schedule);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    // ============================================================
    // EDIT
    // ============================================================

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
            return NotFound();

        await PopulateSelectListsAsync(schedule);

        return View(schedule);
    }


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

        var validationResult =
            await ValidateScheduleAsync(
                schedule,
                confirmTeacherSubject);

        if (!validationResult.IsValid)
        {
            await PopulateSelectListsAsync(schedule);
            return View(schedule);
        }

        await SetTeacherSubjectWarningAsync(schedule);

        if (ViewBag.TeacherSubjectWarning == true &&
            !confirmTeacherSubject)
        {
            await PopulateSelectListsAsync(schedule);
            return View(schedule);
        }

        try
        {
            _context.Update(schedule);

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


    // ============================================================
    // DELETE
    // ============================================================

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
            return NotFound();

        return View(schedule);
    }


    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        var schedule =
            await _context.Schedules
                .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule != null)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // EXPORT QUERIES
    // ============================================================

    private async Task<List<Schedule>>
        GetAllSchedulesForExportAsync()
    {
        return await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .ToListAsync();
    }


    private async Task<List<Schedule>>
        GetGroupSchedulesForExportAsync(int groupId)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .Where(s => s.GroupId == groupId)
            .ToListAsync();
    }


    private async Task<List<Schedule>>
        GetTeacherSchedulesForExportAsync(int teacherId)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .Where(s => s.TeacherId == teacherId)
            .ToListAsync();
    }


    // ============================================================
    // FIND SCHEDULE
    // ============================================================

    private async Task<Schedule?> FindScheduleAsync(
        int id)
    {
        return await _context.Schedules
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .Include(s => s.Subject)
            .Include(s => s.Classroom)
            .FirstOrDefaultAsync(s => s.Id == id);
    }


    // ============================================================
    // SELECT LISTS
    // ============================================================

    private async Task PopulateSelectListsAsync(
        Schedule? schedule = null)
    {
        ViewBag.TeacherId =
            new SelectList(
                await _context.Teachers
                    .AsNoTracking()
                    .OrderBy(t => t.FullName)
                    .ToListAsync(),
                "Id",
                "FullName",
                schedule?.TeacherId);

        ViewBag.GroupId =
            new SelectList(
                await _context.Groups
                    .AsNoTracking()
                    .OrderBy(g => g.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                schedule?.GroupId);

        ViewBag.SubjectId =
            new SelectList(
                await _context.Subjects
                    .AsNoTracking()
                    .OrderBy(s => s.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                schedule?.SubjectId);

        ViewBag.ClassroomId =
            new SelectList(
                await _context.Classrooms
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                schedule?.ClassroomId);

        ViewBag.DayOfWeek =
        new SelectList(
            new[]
            {
            new { Value = DayOfWeek.Monday, Name = "Понедельник" },
            new { Value = DayOfWeek.Tuesday, Name = "Вторник" },
            new { Value = DayOfWeek.Wednesday, Name = "Среда" },
            new { Value = DayOfWeek.Thursday, Name = "Четверг" },
            new { Value = DayOfWeek.Friday, Name = "Пятница" },
            new { Value = DayOfWeek.Saturday, Name = "Суббота" },
            new { Value = DayOfWeek.Sunday, Name = "Воскресенье" }
            },
            "Value",
            "Name",
            schedule?.DayOfWeek);
    }


    // ============================================================
    // VALIDATION
    // ============================================================

    private void RemoveDefaultValidationErrors()
    {
        ModelState.Remove("Teacher");
        ModelState.Remove("Group");
        ModelState.Remove("Subject");
        ModelState.Remove("Classroom");
    }


    private async Task<ValidationResult>
        ValidateScheduleAsync(
            Schedule schedule,
            bool confirmTeacherSubject)
    {
        if (!ValidateBasicData(schedule))
        {
            return new ValidationResult(false);
        }

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Id == schedule.TeacherId);

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                g => g.Id == schedule.GroupId);

        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == schedule.SubjectId);

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(
                c => c.Id == schedule.ClassroomId);

        if (teacher == null)
        {
            ModelState.AddModelError(
                "TeacherId",
                "Преподаватель не найден.");
        }

        if (group == null)
        {
            ModelState.AddModelError(
                "GroupId",
                "Группа не найдена.");
        }

        if (subject == null)
        {
            ModelState.AddModelError(
                "SubjectId",
                "Предмет не найден.");
        }

        if (classroom == null)
        {
            ModelState.AddModelError(
                "ClassroomId",
                "Аудитория не найдена.");
        }

        if (!ModelState.IsValid)
        {
            return new ValidationResult(false);
        }

        // --------------------------------------------------------
        // Преподаватель / предмет
        // --------------------------------------------------------

        if (teacher!.SubjectId.HasValue &&
            teacher.SubjectId.Value != subject!.Id)
        {
            ViewBag.TeacherSubjectWarning = true;

            ViewBag.TeacherName = teacher.FullName;

            var teacherSubject = await _context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id == teacher.SubjectId.Value);

            ViewBag.TeacherSubjectName =
                teacherSubject?.Name ?? "неизвестный предмет";

            ViewBag.SelectedSubjectName =
                subject.Name;

            if (!confirmTeacherSubject)
            {
                ModelState.AddModelError(
                    "",
                    $"Преподаватель «{teacher.FullName}» обычно ведёт " +
                    $"другой предмет. Проверьте выбор.");
            }
        }


        // --------------------------------------------------------
        // Вместимость группы / аудитории
        // --------------------------------------------------------

        if (group!.StudentCount > classroom!.Capacity)
        {
            ModelState.AddModelError(
                "ClassroomId",
                $"В аудитории «{classroom.Name}» недостаточно мест. " +
                $"Вместимость: {classroom.Capacity}, " +
                $"в группе: {group.StudentCount} человек.");

            await BuildClassroomRecommendationsAsync(
                schedule,
                subject!,
                group!);
        }


        // --------------------------------------------------------
        // Требование компьютеров
        // --------------------------------------------------------

        if (subject!.RequiresComputers &&
            !classroom.HasComputers)
        {
            ModelState.AddModelError(
                "ClassroomId",
                $"Предмет «{subject.Name}» требует компьютерную " +
                $"аудиторию. В аудитории «{classroom.Name}» " +
                $"компьютеров нет.");

            await BuildClassroomRecommendationsAsync(
                schedule,
                subject,
                group);
        }


        // --------------------------------------------------------
        // Конфликт аудитории
        // --------------------------------------------------------

        if (await HasClassroomConflictAsync(schedule))
        {
            ModelState.AddModelError(
                "ClassroomId",
                $"Аудитория «{classroom.Name}» уже занята " +
                $"в выбранное время.");

            await BuildClassroomRecommendationsAsync(
                schedule,
                subject,
                group);
        }


        // --------------------------------------------------------
        // Конфликт группы
        // --------------------------------------------------------

        if (await HasGroupConflictAsync(schedule))
        {
            ModelState.AddModelError(
                "GroupId",
                $"У группы «{group.Name}» уже есть занятие " +
                $"в выбранное время.");
        }


        // --------------------------------------------------------
        // Конфликт преподавателя
        // --------------------------------------------------------

        if (await HasTeacherConflictAsync(schedule))
        {
            ModelState.AddModelError(
                "TeacherId",
                $"У преподавателя «{teacher.FullName}» уже есть " +
                $"занятие в выбранное время.");
        }


        return new ValidationResult(
            ModelState.IsValid);
    }


    private bool ValidateBasicData(
        Schedule schedule)
    {
        if (!schedule.DayOfWeek.HasValue)
        {
            ModelState.AddModelError(
                "DayOfWeek",
                "Выберите день недели.");
        }

        if (!schedule.StartTime.HasValue ||
            !schedule.EndTime.HasValue)
        {
            ModelState.AddModelError(
                "",
                "Укажите время начала и окончания занятия.");

            return false;
        }

        if (schedule.StartTime >= schedule.EndTime)
        {
            ModelState.AddModelError(
                "",
                "Время окончания должно быть позже времени начала.");
        }

        return ModelState.IsValid;
    }


    // ============================================================
    // CONFLICTS
    // ============================================================

    private async Task<bool> HasClassroomConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id != schedule.Id &&
                s.ClassroomId == schedule.ClassroomId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }


    private async Task<bool> HasGroupConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id != schedule.Id &&
                s.GroupId == schedule.GroupId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }


    private async Task<bool> HasTeacherConflictAsync(
        Schedule schedule)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id != schedule.Id &&
                s.TeacherId == schedule.TeacherId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime);
    }


    // ============================================================
    // CLASSROOM RECOMMENDATIONS
    // ============================================================

    private async Task BuildClassroomRecommendationsAsync(
        Schedule schedule,
        Subject subject,
        Group group)
    {
        var classrooms = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .Where(c =>
                c.Id != schedule.ClassroomId &&
                c.Capacity >= group.StudentCount &&
                (!subject.RequiresComputers ||
                 c.HasComputers))
            .OrderBy(c => c.Capacity)
            .ThenBy(c => c.Name)
            .ToListAsync();

        var recommendedClassrooms =
            new List<Classroom>();

        foreach (var classroom in classrooms)
        {
            var busy = await _context.Schedules
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Id != schedule.Id &&
                    s.ClassroomId == classroom.Id &&
                    s.DayOfWeek == schedule.DayOfWeek &&
                    s.StartTime < schedule.EndTime &&
                    s.EndTime > schedule.StartTime);

            if (!busy)
            {
                recommendedClassrooms.Add(classroom);
            }
        }

        ViewBag.RecommendedClassrooms =
            recommendedClassrooms;

        var selectedClassroom =
            await _context.Classrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Id == schedule.ClassroomId);

        ViewBag.SelectedClassroomName =
            selectedClassroom?.Name;
    }


    // ============================================================
    // TEACHER / SUBJECT WARNING
    // ============================================================

    private async Task SetTeacherSubjectWarningAsync(
        Schedule schedule)
    {
        if (schedule.TeacherId <= 0 ||
            schedule.SubjectId <= 0)
        {
            return;
        }

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Id == schedule.TeacherId);

        if (teacher == null)
            return;

        ViewBag.TeacherSubjectWarning =
            teacher.SubjectId.HasValue &&
            teacher.SubjectId.Value != schedule.SubjectId;
    }


    // ============================================================
    // EXISTS
    // ============================================================

    private async Task<bool> ScheduleExistsAsync(
        int id)
    {
        return await _context.Schedules
            .AnyAsync(e => e.Id == id);
    }


    // ============================================================
    // FILE NAME
    // ============================================================

    private static string MakeSafeFileName(
        string fileName)
    {
        var invalidChars =
            Path.GetInvalidFileNameChars();

        var result = new string(
            fileName
                .Select(c =>
                    invalidChars.Contains(c)
                        ? '_'
                        : c)
                .ToArray());

        return string.IsNullOrWhiteSpace(result)
            ? "Расписание"
            : result;
    }


    // ============================================================
    // VALIDATION RESULT
    // ============================================================

    private sealed class ValidationResult
    {
        public bool IsValid { get; }

        public ValidationResult(bool isValid)
        {
            IsValid = isValid;
        }
    }
}
