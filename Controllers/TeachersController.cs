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
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
        {
            return NotFound();
        }

        return View(teacher);
    }

    // GET: Teachers/Create
    public async Task<IActionResult> Create()
    {
        await PopulateSubjectsAsync();

        return View();
    }

    // POST: Teachers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,FullName,SubjectId,Email")] Teacher teacher)
    {
        if (ModelState.IsValid)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await PopulateSubjectsAsync(teacher.SubjectId);

        return View(teacher);
    }

    // GET: Teachers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers.FindAsync(id);

        if (teacher == null)
        {
            return NotFound();
        }

        await PopulateSubjectsAsync(teacher.SubjectId);

        return View(teacher);
    }

    // POST: Teachers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,FullName,SubjectId,Email")] Teacher teacher)
    {
        if (id != teacher.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Teachers.Update(teacher);
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

            return RedirectToAction(nameof(Index));
        }

        await PopulateSubjectsAsync(teacher.SubjectId);

        return View(teacher);
    }

    // GET: Teachers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
        {
            return NotFound();
        }

        return View(teacher);
    }

    // POST: Teachers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);

        if (teacher != null)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSubjectsAsync(int? selectedSubjectId = null)
    {
        ViewData["SubjectId"] = new SelectList(
            await _context.Subjects
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
