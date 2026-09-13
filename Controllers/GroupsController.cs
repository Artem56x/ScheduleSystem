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
        var groups = _context.Groups.AsQueryable();

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
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    // GET: Groups/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Groups/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,Specialty,StudentCount,Description")] Group group)
    {
        if (ModelState.IsValid)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(group);
    }

    // GET: Groups/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var group = await _context.Groups.FindAsync(id);

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    // POST: Groups/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Specialty,StudentCount,Description")] Group group)
    {
        if (id != group.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Groups.Update(group);
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

            return RedirectToAction(nameof(Index));
        }

        return View(group);
    }

    // GET: Groups/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    // POST: Groups/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var group = await _context.Groups.FindAsync(id);

        if (group != null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool GroupExists(int id)
    {
        return _context.Groups.Any(g => g.Id == id);
    }
}

