using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Controllers;

public class SubjectsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SubjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Subjects
    public async Task<IActionResult> Index(string? search)
    {
        var subjects = _context.Subjects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            subjects = subjects.Where(s =>
                s.Name.Contains(search));
        }

        ViewBag.Search = search;

        return View(await subjects
            .OrderBy(s => s.Name)
            .ToListAsync());
    }

    // GET: Subjects/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return NotFound();
        }

        return View(subject);
    }

    // GET: Subjects/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Subjects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,RequiresComputers")] Subject subject)
    {
        if (!ModelState.IsValid)
        {
            return View(subject);
        }

        _context.Subjects.Add(subject);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Subjects/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return NotFound();
        }

        return View(subject);
    }

    // POST: Subjects/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,RequiresComputers")] Subject subject)
    {
        if (id != subject.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(subject);
        }

        var existingSubject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSubject == null)
        {
            return NotFound();
        }

        existingSubject.Name = subject.Name;
        existingSubject.RequiresComputers = subject.RequiresComputers;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SubjectExists(subject.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Subjects/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return NotFound();
        }

        return View(subject);
    }

    // POST: Subjects/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);

        if (subject == null)
        {
            return NotFound();
        }

        // Проверяем использование предмета в расписании
        var isUsedInSchedule = await _context.Schedules
            .AsNoTracking()
            .AnyAsync(schedule => schedule.SubjectId == id);

        if (isUsedInSchedule)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить предмет: он используется в расписании.";

            return RedirectToAction(nameof(Index));
        }

        // Проверяем использование предмета у преподавателей
        var isUsedByTeacher = await _context.Teachers
            .AsNoTracking()
            .AnyAsync(teacher => teacher.SubjectId == id);

        if (isUsedByTeacher)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить предмет: он указан у одного или нескольких преподавателей.";

            return RedirectToAction(nameof(Index));
        }

        _context.Subjects.Remove(subject);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить предмет: он используется в других данных системы.";

            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    private bool SubjectExists(int id)
    {
        return _context.Subjects.Any(e => e.Id == id);
    }
}

