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
            .Include(c => c.ClassroomCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null)
        {
            return NotFound();
        }

        return View(classroom);
    }

    // GET: Classrooms/Create
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();

        return View();
    }

    // POST: Classrooms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,ClassroomCategoryId,Capacity,HasComputers")]
    Classroom classroom)
    {
        // Проверяем категорию
        if (classroom.ClassroomCategoryId <= 0)
        {
            ModelState.AddModelError(
                nameof(classroom.ClassroomCategoryId),
                "Выберите категорию аудитории.");
        }
        else
        {
            var categoryExists = await _context.ClassroomCategories
                .AnyAsync(c => c.Id == classroom.ClassroomCategoryId);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(classroom.ClassroomCategoryId),
                    "Выбранная категория не существует.");
            }
        }

        if (ModelState.IsValid)
        {
            _context.Classrooms.Add(classroom);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await LoadCategoriesAsync(classroom.ClassroomCategoryId);

        return View(classroom);
    }

    // POST: Classrooms/CreateCategory
    // Создание категории прямо из формы добавления аудитории
    [HttpPost]
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

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            id = category.Id,
            name = category.Name
        });
    }

    // GET: Classrooms/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var classroom = await _context.Classrooms
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

        // Проверяем категорию
        if (classroom.ClassroomCategoryId <= 0)
        {
            ModelState.AddModelError(
                nameof(classroom.ClassroomCategoryId),
                "Выберите категорию аудитории.");
        }
        else
        {
            var categoryExists = await _context.ClassroomCategories
                .AnyAsync(c => c.Id == classroom.ClassroomCategoryId);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(classroom.ClassroomCategoryId),
                    "Выбранная категория не существует.");
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Classrooms.Update(classroom);

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

            return RedirectToAction(nameof(Index));
        }

        await LoadCategoriesAsync(classroom.ClassroomCategoryId);

        return View(classroom);
    }

    // GET: Classrooms/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var classroom = await _context.Classrooms
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
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var classroom = await _context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classroom == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Проверяем, используется ли аудитория в расписании
        var isUsed = await _context.Schedules
            .AnyAsync(s => s.ClassroomId == id);

        if (isUsed)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить аудиторию: она используется в расписании.";

            return RedirectToAction(nameof(Index));
        }

        _context.Classrooms.Remove(classroom);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Загружает категории для выпадающего списка
    private async Task LoadCategoriesAsync(int? selectedCategoryId = null)
    {
        var categories = await _context.ClassroomCategories
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
