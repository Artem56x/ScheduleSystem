using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Controllers;

[Authorize]
public class GroupSubjectsController : Controller
{
    private readonly ApplicationDbContext _context;

    public GroupSubjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // INDEX
    // ============================================================

    public async Task<IActionResult> Index(int? groupId)
    {
        var groups = await _context.Groups
            .OrderBy(g => g.Course)
            .ThenBy(g => g.Name)
            .ToListAsync();

        ViewBag.Groups = groups;
        ViewBag.SelectedGroupId = groupId;

        IQueryable<GroupSubject> query = _context.GroupSubjects
            .Include(gs => gs.Group)
            .Include(gs => gs.Subject);

        if (groupId.HasValue)
        {
            query = query.Where(gs => gs.GroupId == groupId.Value);
        }

        var groupSubjects = await query
            .OrderBy(gs => gs.Group!.Course)
            .ThenBy(gs => gs.Group!.Name)
            .ThenBy(gs => gs.Subject!.Name)
            .ToListAsync();

        return View(groupSubjects);
    }

    // ============================================================
    // CREATE - GET
    // ============================================================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(int? groupId)
    {
        var groups = await _context.Groups
            .OrderBy(g => g.Course)
            .ThenBy(g => g.Name)
            .ToListAsync();

        var subjects = await _context.Subjects
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Groups = new SelectList(
            groups,
            "Id",
            "Name",
            groupId
        );

        ViewBag.Subjects = subjects
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} — {GetSubjectTypeName(s.Type)}"
            })
            .ToList();

        var model = new GroupSubject
        {
            GroupId = groupId ?? 0,
            WeeklyLessons = 1
        };

        return View(model);
    }

    // ============================================================
    // CREATE - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(GroupSubject model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareCreateViewAsync(model);
            return View(model);
        }

        var groupExists = await _context.Groups
            .AnyAsync(g => g.Id == model.GroupId);

        if (!groupExists)
        {
            ModelState.AddModelError(
                nameof(model.GroupId),
                "Выбранная группа не существует."
            );

            await PrepareCreateViewAsync(model);
            return View(model);
        }

        var subjectExists = await _context.Subjects
            .AnyAsync(s => s.Id == model.SubjectId);

        if (!subjectExists)
        {
            ModelState.AddModelError(
                nameof(model.SubjectId),
                "Выбранный предмет не существует."
            );

            await PrepareCreateViewAsync(model);
            return View(model);
        }

        var duplicate = await _context.GroupSubjects
            .AnyAsync(gs =>
                gs.GroupId == model.GroupId &&
                gs.SubjectId == model.SubjectId);

        if (duplicate)
        {
            ModelState.AddModelError(
                nameof(model.SubjectId),
                "Этот предмет уже добавлен для выбранной группы."
            );

            await PrepareCreateViewAsync(model);
            return View(model);
        }

        _context.GroupSubjects.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Предмет успешно добавлен в учебную нагрузку.";

        return RedirectToAction(
            nameof(Index),
            new { groupId = model.GroupId }
        );
    }

    // ============================================================
    // EDIT - GET
    // ============================================================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var model = await _context.GroupSubjects
            .Include(gs => gs.Group)
            .Include(gs => gs.Subject)
            .FirstOrDefaultAsync(gs => gs.Id == id);

        if (model == null)
        {
            return NotFound();
        }

        await PrepareEditViewAsync(model);

        return View(model);
    }

    // ============================================================
    // EDIT - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(
        int id,
        GroupSubject model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PrepareEditViewAsync(model);
            return View(model);
        }

        var existing = await _context.GroupSubjects
            .FirstOrDefaultAsync(gs => gs.Id == id);

        if (existing == null)
        {
            return NotFound();
        }

        var duplicate = await _context.GroupSubjects
            .AnyAsync(gs =>
                gs.Id != id &&
                gs.GroupId == model.GroupId &&
                gs.SubjectId == model.SubjectId);

        if (duplicate)
        {
            ModelState.AddModelError(
                nameof(model.SubjectId),
                "Этот предмет уже добавлен для выбранной группы."
            );

            await PrepareEditViewAsync(model);
            return View(model);
        }

        existing.GroupId = model.GroupId;
        existing.SubjectId = model.SubjectId;
        existing.WeeklyLessons = model.WeeklyLessons;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Учебная нагрузка успешно изменена.";

        return RedirectToAction(
            nameof(Index),
            new { groupId = model.GroupId }
        );
    }

    // ============================================================
    // DELETE - GET
    // ============================================================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var model = await _context.GroupSubjects
            .Include(gs => gs.Group)
            .Include(gs => gs.Subject)
            .FirstOrDefaultAsync(gs => gs.Id == id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // ============================================================
    // DELETE - POST
    // ============================================================

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var model = await _context.GroupSubjects
            .FirstOrDefaultAsync(gs => gs.Id == id);

        if (model == null)
        {
            return NotFound();
        }

        var groupId = model.GroupId;

        _context.GroupSubjects.Remove(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Предмет удалён из учебной нагрузки.";

        return RedirectToAction(
            nameof(Index),
            new { groupId }
        );
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private async Task PrepareCreateViewAsync(GroupSubject model)
    {
        var groups = await _context.Groups
            .OrderBy(g => g.Course)
            .ThenBy(g => g.Name)
            .ToListAsync();

        var subjects = await _context.Subjects
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Groups = new SelectList(
            groups,
            "Id",
            "Name",
            model.GroupId
        );

        ViewBag.Subjects = subjects
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} — {GetSubjectTypeName(s.Type)}",
                Selected = s.Id == model.SubjectId
            })
            .ToList();
    }

    private async Task PrepareEditViewAsync(GroupSubject model)
    {
        await PrepareCreateViewAsync(model);
    }

    private static string GetSubjectTypeName(SubjectType type)
    {
        return type switch
        {
            SubjectType.Special => "Специальный",
            _ => "Общеобразовательный"
        };
    }
}