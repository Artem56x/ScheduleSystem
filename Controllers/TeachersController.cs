using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
using ScheduleSystem.ViewModels;
namespace ScheduleSystem.Controllers;



public class TeachersController : Controller
{
    private readonly ApplicationDbContext _context;

    public TeachersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Teachers
    public async Task<IActionResult> Index(string? search)
    {
        var teachers = _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            teachers = teachers.Where(t =>
                t.FullName.Contains(search) ||
                t.TeacherSubjects.Any(ts =>
                    ts.Subject.Name.Contains(search)));
        }

        var result = await teachers
            .OrderBy(t => t.FullName)
            .ToListAsync();

        ViewBag.Search = search;

        return View(result);
    }

    // GET: Teachers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
        {
            return NotFound();
        }

        return View(teacher);
    }

    // GET: Teachers/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await PopulateSubjectsAsync();

        return View();
    }

    // POST: Teachers/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,FullName,Email")]
        Teacher teacher,
        int[]? subjectIds)
    {
        subjectIds ??= Array.Empty<int>();

        // Убираем дубликаты
        subjectIds = subjectIds
            .Distinct()
            .ToArray();

        // Проверяем, что выбранные предметы существуют
        var validSubjectIds = await _context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        if (validSubjectIds.Count != subjectIds.Length)
        {
            ModelState.AddModelError(
                string.Empty,
                "Один или несколько выбранных предметов не существуют.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSubjectsAsync(subjectIds);

            return View(teacher);
        }

        foreach (var subjectId in subjectIds)
        {
            teacher.TeacherSubjects.Add(new TeacherSubject
            {
                SubjectId = subjectId
            });
        }

        _context.Teachers.Add(teacher);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось создать преподавателя. Проверьте введённые данные.");

            await PopulateSubjectsAsync(subjectIds);

            return View(teacher);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Teachers/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
        {
            return NotFound();
        }

        var subjectIds = teacher.TeacherSubjects
            .Select(ts => ts.SubjectId)
            .ToArray();

        await PopulateSubjectsAsync(subjectIds);

        return View(teacher);
    }

    // POST: Teachers/Edit/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,FullName,Email")]
        Teacher teacher,
        int[]? subjectIds)
    {
        if (id != teacher.Id)
        {
            return NotFound();
        }

        subjectIds ??= Array.Empty<int>();

        subjectIds = subjectIds
            .Distinct()
            .ToArray();

        // Проверяем существование выбранных предметов
        var validSubjectIds = await _context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        if (validSubjectIds.Count != subjectIds.Length)
        {
            ModelState.AddModelError(
                string.Empty,
                "Один или несколько выбранных предметов не существуют.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSubjectsAsync(subjectIds);

            return View(teacher);
        }

        var existingTeacher = await _context.Teachers
            .Include(t => t.TeacherSubjects)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (existingTeacher == null)
        {
            return NotFound();
        }

        existingTeacher.FullName = teacher.FullName;
        existingTeacher.Email = teacher.Email;

        // Удаляем старые связи
        existingTeacher.TeacherSubjects.Clear();

        // Добавляем новые связи
        foreach (var subjectId in subjectIds)
        {
            existingTeacher.TeacherSubjects.Add(new TeacherSubject
            {
                TeacherId = existingTeacher.Id,
                SubjectId = subjectId
            });
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TeacherExists(teacher.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось сохранить изменения преподавателя.");

            await PopulateSubjectsAsync(subjectIds);

            return View(teacher);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Teachers/Delete/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
        {
            return NotFound();
        }

        return View(teacher);
    }

    // POST: Teachers/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var teacher = await _context.Teachers
            .FindAsync(id);

        if (teacher == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Проверяем, используется ли преподаватель в расписании
        var isUsedInSchedule = await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s => s.TeacherId == id);

        if (isUsedInSchedule)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить преподавателя: он используется в расписании.";

            return RedirectToAction(nameof(Index));
        }

        _context.Teachers.Remove(teacher);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить преподавателя: он используется в других данных системы.";

            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    // Список предметов для Create/Edit
    private async Task PopulateSubjectsAsync(
        IEnumerable<int>? selectedSubjectIds = null)
    {
        var selectedIds = selectedSubjectIds?
            .ToHashSet()
            ?? new HashSet<int>();

        var subjects = await _context.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Course)
            .ThenBy(s => s.Name)
            .ToListAsync();

        ViewBag.Subjects = subjects
            .Select(subject => new SubjectSelectionViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Course = subject.Course,
                IsSelected = selectedIds.Contains(subject.Id)
            })
            .ToList();
    }

    private bool TeacherExists(int id)
    {
        return _context.Teachers
            .Any(e => e.Id == id);
    }
}