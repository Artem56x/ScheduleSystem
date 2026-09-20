using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Controllers;

public class GroupsController : Controller
{
    private readonly ApplicationDbContext _context;

    public GroupsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Groups
    public async Task<IActionResult> Index(string? search)
    {
        var groups = _context.Groups
            .AsNoTracking()
            .AsQueryable();

        // Поиск по названию группы и специальности
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            groups = groups.Where(g =>
                g.Name.Contains(search) ||
                g.Specialty.Contains(search));
        }

        var result = await groups
            .OrderBy(g => g.Name)
            .ToListAsync();

        ViewBag.Search = search;

        return View(result);
    }

    // GET: Groups/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    // GET: Groups/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Groups/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,Specialty,Course,StudentCount,Description")]
        Group group)
    {
        if (!ModelState.IsValid)
        {
            return View(group);
        }

        _context.Groups.Add(group);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось создать группу. Проверьте введённые данные.");

            return View(group);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Groups/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    // POST: Groups/Edit/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Specialty,Course,StudentCount,Description")]
        Group group)
    {
        if (id != group.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(group);
        }

        var existingGroup = await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == id);

        if (existingGroup == null)
        {
            return NotFound();
        }

        existingGroup.Name = group.Name;
        existingGroup.Specialty = group.Specialty;
        existingGroup.Course = group.Course;
        existingGroup.StudentCount = group.StudentCount;
        existingGroup.Description = group.Description;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GroupExists(group.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось сохранить изменения группы.");

            return View(group);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Groups/Delete/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var group = await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    // POST: Groups/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var group = await _context.Groups
            .FindAsync(id);

        if (group == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Проверяем, используется ли группа в расписании
        var isUsedInSchedule = await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s => s.GroupId == id);

        if (isUsedInSchedule)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить группу: она используется в расписании.";

            return RedirectToAction(nameof(Index));
        }

        _context.Groups.Remove(group);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить группу: она используется в других данных системы.";

            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    private bool GroupExists(int id)
    {
        return _context.Groups.Any(g => g.Id == id);
    }
}

