using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
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
            .Include(t => t.Subject)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            teachers = teachers.Where(t =>
                t.FullName.Contains(search) ||
                (t.Email != null && t.Email.Contains(search)) ||
                (t.Subject != null && t.Subject.Name.Contains(search)));
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
            .Include(t => t.Subject)
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
            [Bind("Id,FullName,SubjectId,Email")]
        Teacher teacher)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSubjectsAsync(teacher.SubjectId);

            return View(teacher);
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

            await PopulateSubjectsAsync(teacher.SubjectId);

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
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
        {
            return NotFound();
        }

        await PopulateSubjectsAsync(teacher.SubjectId);

        return View(teacher);
    }

    // POST: Teachers/Edit/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
          int id,
          [Bind("Id,FullName,SubjectId,Email")]
        Teacher teacher)
    {
        if (id != teacher.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateSubjectsAsync(teacher.SubjectId);

            return View(teacher);
        }

        var existingTeacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == id);

        if (existingTeacher == null)
        {
            return NotFound();
        }

        existingTeacher.FullName = teacher.FullName;
        existingTeacher.SubjectId = teacher.SubjectId;
        existingTeacher.Email = teacher.Email;

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

            await PopulateSubjectsAsync(teacher.SubjectId);

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
            .Include(t => t.Subject)
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

    private async Task PopulateSubjectsAsync(
        int? selectedSubjectId = null)
    {
        ViewData["SubjectId"] = new SelectList(
            await _context.Subjects
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync(),
            "Id",
            "Name",
            selectedSubjectId);
    }

    private bool TeacherExists(int id)
    {
        return _context.Teachers.Any(e => e.Id == id);
    }
}