using Microsoft.AspNetCore.Authorization;
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

    // ============================================================
    // INDEX
    // ============================================================

    public async Task<IActionResult> Index(string? search)
    {
        var subjects = _context.Subjects
            .AsNoTracking()
            .Include(s => s.ClassroomCategoryRequirements)
                .ThenInclude(x => x.ClassroomCategory)
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

    // ============================================================
    // DETAILS
    // ============================================================

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subject = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.ClassroomCategoryRequirements)
                .ThenInclude(x => x.ClassroomCategory)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return NotFound();
        }

        return View(subject);
    }

    // ============================================================
    // CREATE - GET
    // ============================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await LoadClassroomCategoriesAsync();

        return View(new Subject());
    }

    // ============================================================
    // CREATE - POST
    // ============================================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,Course,Type")]
        Subject subject,
        int[]? selectedClassroomCategoryIds)
    {
        var categoryIds = selectedClassroomCategoryIds?
            .Distinct()
            .ToList()
            ?? new List<int>();

        if (!ModelState.IsValid)
        {
            await LoadClassroomCategoriesAsync();

            ViewBag.SelectedClassroomCategoryIds = categoryIds;

            return View(subject);
        }

        // Проверяем, что все переданные категории действительно существуют
        if (categoryIds.Count > 0)
        {
            var validCategoryIds = await _context.ClassroomCategories
                .AsNoTracking()
                .Where(c => categoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (validCategoryIds.Count != categoryIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Одна или несколько выбранных категорий аудиторий не существуют.");

                await LoadClassroomCategoriesAsync();

                ViewBag.SelectedClassroomCategoryIds = categoryIds;

                return View(subject);
            }
        }

        _context.Subjects.Add(subject);

        foreach (var categoryId in categoryIds)
        {
            subject.ClassroomCategoryRequirements.Add(
                new SubjectClassroomCategory
                {
                    Subject = subject,
                    ClassroomCategoryId = categoryId
                });
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // EDIT - GET
    // ============================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subject = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.ClassroomCategoryRequirements)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return NotFound();
        }

        await LoadClassroomCategoriesAsync();

        ViewBag.SelectedClassroomCategoryIds =
            subject.ClassroomCategoryRequirements
                .Select(x => x.ClassroomCategoryId)
                .ToList();

        return View(subject);
    }

    // ============================================================
    // EDIT - POST
    // ============================================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Course,Type")]
        Subject subject,
        int[]? selectedClassroomCategoryIds)
    {
        if (id != subject.Id)
        {
            return NotFound();
        }

        var categoryIds = selectedClassroomCategoryIds?
            .Distinct()
            .ToList()
            ?? new List<int>();

        if (!ModelState.IsValid)
        {
            await LoadClassroomCategoriesAsync();

            ViewBag.SelectedClassroomCategoryIds = categoryIds;

            return View(subject);
        }

        var existingSubject = await _context.Subjects
            .Include(s => s.ClassroomCategoryRequirements)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSubject == null)
        {
            return NotFound();
        }

        // Проверяем существование выбранных категорий
        if (categoryIds.Count > 0)
        {
            var validCategoryIds = await _context.ClassroomCategories
                .AsNoTracking()
                .Where(c => categoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (validCategoryIds.Count != categoryIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Одна или несколько выбранных категорий аудиторий не существуют.");

                await LoadClassroomCategoriesAsync();

                ViewBag.SelectedClassroomCategoryIds = categoryIds;

                return View(subject);
            }
        }

        // Обновляем основные данные
        existingSubject.Name = subject.Name;
        existingSubject.Course = subject.Course;
        existingSubject.Type = subject.Type;

        // Полностью пересобираем требования к аудиториям
        existingSubject.ClassroomCategoryRequirements.Clear();

        foreach (var categoryId in categoryIds)
        {
            existingSubject.ClassroomCategoryRequirements.Add(
                new SubjectClassroomCategory
                {
                    SubjectId = existingSubject.Id,
                    ClassroomCategoryId = categoryId
                });
        }

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

    // ============================================================
    // DELETE - GET
    // ============================================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var subject = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.ClassroomCategoryRequirements)
                .ThenInclude(x => x.ClassroomCategory)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return NotFound();
        }

        return View(subject);
    }

    // ============================================================
    // DELETE - POST
    // ============================================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subject = await _context.Subjects
            .Include(s => s.ClassroomCategoryRequirements)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return NotFound();
        }

        // Проверяем использование в расписании
        var isUsedInSchedule = await _context.Schedules
            .AsNoTracking()
            .AnyAsync(schedule => schedule.SubjectId == id);

        if (isUsedInSchedule)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить предмет: он используется в расписании.";

            return RedirectToAction(nameof(Index));
        }

        // Проверяем использование преподавателями
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

    // ============================================================
    // HELPERS
    // ============================================================

    private async Task LoadClassroomCategoriesAsync()
    {
        ViewBag.ClassroomCategories = await _context.ClassroomCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    private bool SubjectExists(int id)
    {
        return _context.Subjects
            .Any(e => e.Id == id);
    }
}