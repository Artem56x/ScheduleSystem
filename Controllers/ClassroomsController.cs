using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
namespace ScheduleSystem.Controllers;

public class ClassroomsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClassroomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Classrooms
    public async Task<IActionResult> Index(string? search)
    {
        var classrooms = _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .AsQueryable();

        // Поиск по номеру аудитории и категории
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            classrooms = classrooms.Where(c =>
                c.Name.Contains(search) ||
                (c.ClassroomCategory != null &&
                 c.ClassroomCategory.Name.Contains(search)));
        }

        var result = await classrooms
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Search = search;

        return View(result);
    }

    // GET: Classrooms/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null)
        {
            return NotFound();
        }

        return View(classroom);
    }

    // GET: Classrooms/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();

        return View();
    }

    // POST: Classrooms/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
            [Bind("Id,Name,ClassroomCategoryId,Capacity,HasComputers")]
        Classroom classroom)
    {
        await ValidateCategoryAsync(classroom.ClassroomCategoryId);

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(classroom.ClassroomCategoryId);

            return View(classroom);
        }

        _context.Classrooms.Add(classroom);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось создать аудиторию. Проверьте введённые данные.");

            await LoadCategoriesAsync(classroom.ClassroomCategoryId);

            return View(classroom);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Classrooms/CreateCategory
    // Создание категории прямо из формы добавления аудитории
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                success = false,
                message = "Введите название категории."
            });
        }

        var name = request.Name.Trim();

        // Проверяем, нет ли такой категории
        var exists = await _context.ClassroomCategories
            .AsNoTracking()
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return BadRequest(new
            {
                success = false,
                message = "Такая категория уже существует."
            });
        }

        var category = new ClassroomCategory
        {
            Name = name
        };

        _context.ClassroomCategories.Add(category);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new
            {
                success = false,
                message = "Не удалось создать категорию. Возможно, такая категория уже существует."
            });
        }

        return Ok(new
        {
            success = true,
            id = category.Id,
            name = category.Name
        });
    }

    // GET: Classrooms/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null)
        {
            return NotFound();
        }

        await LoadCategoriesAsync(classroom.ClassroomCategoryId);

        return View(classroom);
    }

    // POST: Classrooms/Edit/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,ClassroomCategoryId,Capacity,HasComputers")]
        Classroom classroom)
    {
        if (id != classroom.Id)
        {
            return NotFound();
        }

        await ValidateCategoryAsync(classroom.ClassroomCategoryId);

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(classroom.ClassroomCategoryId);

            return View(classroom);
        }

        var existingClassroom = await _context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingClassroom == null)
        {
            return NotFound();
        }

        existingClassroom.Name = classroom.Name;
        existingClassroom.ClassroomCategoryId = classroom.ClassroomCategoryId;
        existingClassroom.Capacity = classroom.Capacity;
        existingClassroom.HasComputers = classroom.HasComputers;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClassroomExists(classroom.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось сохранить изменения аудитории.");

            await LoadCategoriesAsync(classroom.ClassroomCategoryId);

            return View(classroom);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Classrooms/Delete/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var classroom = await _context.Classrooms
            .AsNoTracking()
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null)
        {
            return NotFound();
        }

        return View(classroom);
    }

    // POST: Classrooms/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var classroom = await _context.Classrooms
            .FindAsync(id);

        if (classroom == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Проверяем, используется ли аудитория в расписании
        var isUsedInSchedule = await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s => s.ClassroomId == id);

        if (isUsedInSchedule)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить аудиторию: она используется в расписании.";

            return RedirectToAction(nameof(Index));
        }

        _context.Classrooms.Remove(classroom);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить аудиторию: она используется в других данных системы.";

            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    // Проверка существования категории
    private async Task ValidateCategoryAsync(int? categoryId)
    {
        if (!categoryId.HasValue || categoryId.Value <= 0)
        {
            ModelState.AddModelError(
                nameof(Classroom.ClassroomCategoryId),
                "Выберите категорию аудитории.");

            return;
        }

        var categoryExists = await _context.ClassroomCategories
            .AsNoTracking()
            .AnyAsync(c => c.Id == categoryId.Value);

        if (!categoryExists)
        {
            ModelState.AddModelError(
                nameof(Classroom.ClassroomCategoryId),
                "Выбранная категория не существует.");
        }
    }

    // Загружает категории для выпадающего списка
    private async Task LoadCategoriesAsync(int? selectedCategoryId = null)
    {
        var categories = await _context.ClassroomCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.ClassroomCategories =
            new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                categories,
                "Id",
                "Name",
                selectedCategoryId
            );
    }

    private bool ClassroomExists(int id)
    {
        return _context.Classrooms.Any(c => c.Id == id);
    }

    // Модель запроса для создания категории
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}