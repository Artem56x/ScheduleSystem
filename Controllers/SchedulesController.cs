using Microsoft.AspNetCore.Authorization;
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
    private readonly ScheduleValidationService _validationService;
    private readonly ClassroomRecommendationService _classroomRecommendationService;

    public SchedulesController(
        ApplicationDbContext context,
        ScheduleExportService exportService,
        ScheduleValidationService validationService,
        ClassroomRecommendationService classroomRecommendationService)
    {
        _context = context;
        _exportService = exportService;
        _validationService = validationService;
        _classroomRecommendationService = classroomRecommendationService;
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
        {
            return NotFound();
        }

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
        {
            return NotFound();
        }

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

        var fileBytes = _exportService.CreateAllExcel(schedules);

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
        {
            return NotFound();
        }

        var schedules = await GetGroupSchedulesForExportAsync(id);

        var fileBytes = _exportService.CreateGroupExcel(
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
        {
            return NotFound();
        }

        var schedules =
            await GetTeacherSchedulesForExportAsync(id);

        var fileBytes = _exportService.CreateTeacherExcel(
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
        var schedules = await GetAllSchedulesForExportAsync();

        var fileBytes = _exportService.CreateAllPdf(schedules);

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
        {
            return NotFound();
        }

        var schedules = await GetGroupSchedulesForExportAsync(id);

        var fileBytes = _exportService.CreateGroupPdf(
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
        {
            return NotFound();
        }

        var schedules =
            await GetTeacherSchedulesForExportAsync(id);

        var fileBytes = _exportService.CreateTeacherPdf(
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
        var schedules = await GetAllSchedulesForExportAsync();

        var fileBytes = _exportService.CreateAllCsv(schedules);

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
        {
            return NotFound();
        }

        var schedules = await GetGroupSchedulesForExportAsync(id);

        var fileBytes = _exportService.CreateGroupCsv(
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
        {
            return NotFound();
        }

        var schedules =
            await GetTeacherSchedulesForExportAsync(id);

        var fileBytes = _exportService.CreateTeacherCsv(
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
        {
            return NotFound();
        }

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
        {
            return NotFound();
        }

        return View(schedule);
    }


    // ============================================================
    // CREATE
    // ============================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();

        return View();
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
           Schedule schedule,
           bool confirmTeacherSubject = false)
    {
        RemoveDefaultValidationErrors();

        var validationResult =
            await _validationService.ValidateAsync(
                schedule,
                confirmTeacherSubject);

        ApplyValidationResult(validationResult);

        if (!validationResult.IsValid)
        {
            await PrepareValidationViewAsync(
                schedule,
                validationResult);

            return View(schedule);
        }

        try
        {
            _context.Schedules.Add(schedule);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось сохранить расписание. Проверьте выбранные данные и попробуйте ещё раз.");

            await PrepareValidationViewAsync(
                schedule,
                validationResult);

            return View(schedule);
        }

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // EDIT
    // ============================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
        {
            return NotFound();
        }

        await PopulateSelectListsAsync(schedule);

        return View(schedule);
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Schedule schedule,
        bool confirmTeacherSubject = false)
    {
        if (id != schedule.Id)
        {
            return NotFound();
        }

        RemoveDefaultValidationErrors();

        var validationResult =
            await _validationService.ValidateAsync(
                schedule,
                confirmTeacherSubject);

        ApplyValidationResult(validationResult);

        if (!validationResult.IsValid)
        {
            await PrepareValidationViewAsync(
                schedule,
                validationResult);

            return View(schedule);
        }

        var existingSchedule = await _context.Schedules
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSchedule == null)
        {
            return NotFound();
        }

        existingSchedule.TeacherId = schedule.TeacherId;
        existingSchedule.GroupId = schedule.GroupId;
        existingSchedule.SubjectId = schedule.SubjectId;
        existingSchedule.ClassroomId = schedule.ClassroomId;
        existingSchedule.DayOfWeek = schedule.DayOfWeek;
        existingSchedule.StartTime = schedule.StartTime;
        existingSchedule.EndTime = schedule.EndTime;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ScheduleExistsAsync(schedule.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось сохранить изменения расписания.");

            await PrepareValidationViewAsync(
                schedule,
                validationResult);

            return View(schedule);
        }

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // DELETE
    // ============================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var schedule = await FindScheduleAsync(id.Value);

        if (schedule == null)
        {
            return NotFound();
        }

        return View(schedule);
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var schedule = await _context.Schedules
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null)
        {
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Schedules.Remove(schedule);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "Не удалось удалить занятие. Попробуйте ещё раз.";

            return RedirectToAction(nameof(Index));
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
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
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
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
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
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }


    // ============================================================
    // FIND SCHEDULE
    // ============================================================

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


    // ============================================================
    // SELECT LISTS
    // ============================================================

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

        ViewBag.TeacherId = new SelectList(
            teachers,
            "Id",
            "FullName",
            schedule?.TeacherId);

        ViewBag.GroupId = new SelectList(
            groups,
            "Id",
            "Name",
            schedule?.GroupId);

        ViewBag.SubjectId = new SelectList(
            subjects,
            "Id",
            "Name",
            schedule?.SubjectId);

        ViewBag.ClassroomId = new SelectList(
            classrooms,
            "Id",
            "Name",
            schedule?.ClassroomId);

        ViewBag.DayOfWeek = new SelectList(
            new[]
            {
                new
                {
                    Value = DayOfWeek.Monday,
                    Name = "Понедельник"
                },
                new
                {
                    Value = DayOfWeek.Tuesday,
                    Name = "Вторник"
                },
                new
                {
                    Value = DayOfWeek.Wednesday,
                    Name = "Среда"
                },
                new
                {
                    Value = DayOfWeek.Thursday,
                    Name = "Четверг"
                },
                new
                {
                    Value = DayOfWeek.Friday,
                    Name = "Пятница"
                },
                new
                {
                    Value = DayOfWeek.Saturday,
                    Name = "Суббота"
                },
                new
                {
                    Value = DayOfWeek.Sunday,
                    Name = "Воскресенье"
                }
            },
            "Value",
            "Name",
            schedule?.DayOfWeek);
    }


    // ============================================================
    // VALIDATION SUPPORT
    // ============================================================

    private void RemoveDefaultValidationErrors()
    {
        ModelState.Remove("Teacher");
        ModelState.Remove("Group");
        ModelState.Remove("Subject");
        ModelState.Remove("Classroom");
    }


    private void ApplyValidationResult(
        ScheduleValidationResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                error.Key,
                error.Message);
        }

        ViewBag.TeacherSubjectWarning =
            result.TeacherSubjectWarning;

        ViewBag.TeacherName =
            result.TeacherName;

        ViewBag.TeacherSubjectName =
            result.TeacherSubjectName;

        ViewBag.SelectedSubjectName =
            result.SelectedSubjectName;
    }


    private async Task PrepareValidationViewAsync(
        Schedule schedule,
        ScheduleValidationResult validationResult)
    {
        await PopulateSelectListsAsync(schedule);

        if (!validationResult.ClassroomRecommendationNeeded)
        {
            return;
        }

        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == schedule.SubjectId);

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                g => g.Id == schedule.GroupId);

        if (subject == null || group == null)
        {
            return;
        }

        var recommendations =
            await _classroomRecommendationService
                .GetRecommendationsAsync(
                    schedule,
                    subject,
                    group);

        ViewBag.RecommendedClassrooms =
            recommendations;

        ViewBag.SelectedClassroomName =
            await _classroomRecommendationService
                .GetSelectedClassroomNameAsync(
                    schedule.ClassroomId);
    }


    // ============================================================
    // EXISTS
    // ============================================================

    private async Task<bool> ScheduleExistsAsync(int id)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(e => e.Id == id);
    }


    // ============================================================
    // FILE NAME
    // ============================================================

    private static string MakeSafeFileName(string fileName)
    {
        var invalidChars =
            Path.GetInvalidFileNameChars();

        var result = new string(
            fileName.Select(c =>
                invalidChars.Contains(c)
                    ? '_'
                    : c)
            .ToArray());

        return string.IsNullOrWhiteSpace(result)
            ? "Расписание"
            : result;
    }
}

