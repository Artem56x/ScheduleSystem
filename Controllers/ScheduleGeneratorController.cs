using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;
using ScheduleSystem.Models.Generator;
using ScheduleSystem.Services;

namespace ScheduleSystem.Controllers;

[Authorize(Roles = "Admin")]
public class ScheduleGeneratorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ScheduleGeneratorService _generatorService;

    private const string PreviewDataKey =
        "ScheduleGeneratorPreview";

    public ScheduleGeneratorController(
        ApplicationDbContext context,
        ScheduleGeneratorService generatorService)
    {
        _context = context;
        _generatorService = generatorService;
    }

    // ============================================================
    // INDEX
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await LoadViewDataAsync();

        var model = new ScheduleGenerationRequest
        {
            Days = new List<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            },

            TimeSlots = new List<GenerationTimeSlot>
            {

new()
{
    StartTime = new TimeSpan(8, 30, 0),
    EndTime = new TimeSpan(9, 50, 0)
},

new()
{
    StartTime = new TimeSpan(10, 0, 0),
    EndTime = new TimeSpan(11, 20, 0)
},

new()
{
    StartTime = new TimeSpan(11, 40, 0),
    EndTime = new TimeSpan(13, 0, 0)
},

new()
{
    StartTime = new TimeSpan(13, 20, 0),
    EndTime = new TimeSpan(14, 40, 0)
},

new()
{
    StartTime = new TimeSpan(15, 0, 0),
    EndTime = new TimeSpan(16, 20, 0)
},

new()
{
    StartTime = new TimeSpan(16, 40, 0),
    EndTime = new TimeSpan(18, 0, 0)
},

new()
{
    StartTime = new TimeSpan(18, 20, 0),
    EndTime = new TimeSpan(19, 0, 0)
},
            },

            MaxLessonsPerDay = 6,
            MaxConsecutiveLessons = 5,
            DistributeLessons = true,
            UseClassroomRecommendations = true
        };

        return View(model);
    }

    // ============================================================
    // GENERATE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(
        ScheduleGenerationRequest model)
    {
        await LoadViewDataAsync();

        ValidateGenerationRequest(model);

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var result = await _generatorService.GenerateAsync(model);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View("Index", model);
        }

        var preview = new ScheduleGenerationPreviewViewModel
        {
            Items = result.GeneratedItems
        };

        StorePreview(preview);

        return View("Preview", preview);
    }

    // ============================================================
    // PREVIEW
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(
        ScheduleGenerationRequest model)
    {
        await LoadViewDataAsync();

        ValidateGenerationRequest(model);

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var result = await _generatorService.GenerateAsync(model);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View("Index", model);
        }

        var preview = new ScheduleGenerationPreviewViewModel
        {
            Items = result.GeneratedItems
        };

        StorePreview(preview);

        return View(preview);
    }

    // ============================================================
    // SAVE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save()
    {
        // --------------------------------------------------------
        // Получаем предпросмотр
        // --------------------------------------------------------

        var preview = GetStoredPreview();

        if (preview == null ||
            preview.Items == null ||
            preview.Items.Count == 0)
        {
            TempData["ErrorMessage"] =
                "Предпросмотр расписания не найден. " +
                "Сначала выполните генерацию.";

            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------
        // Проверяем данные
        // --------------------------------------------------------

        var items = preview.Items;

        // Удаляем возможные дубликаты внутри самого результата.
        items = items
            .GroupBy(x => new
            {
                x.GroupId,
                x.SubjectId,
                x.TeacherId,
                x.ClassroomId,
                x.DayOfWeek,
                x.StartTime,
                x.EndTime
            })
            .Select(x => x.First())
            .ToList();

        if (items.Count == 0)
        {
            TempData["ErrorMessage"] =
                "В сгенерированном расписании нет занятий.";

            RemoveStoredPreview();

            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------
        // Проверяем существование связанных сущностей
        // --------------------------------------------------------

        var groupIds = items
            .Select(x => x.GroupId)
            .Distinct()
            .ToList();

        var subjectIds = items
            .Select(x => x.SubjectId)
            .Distinct()
            .ToList();

        var teacherIds = items
            .Select(x => x.TeacherId)
            .Distinct()
            .ToList();

        var classroomIds = items
            .Select(x => x.ClassroomId)
            .Distinct()
            .ToList();
        var existingGroupIds = (
            await _context.Groups
                .AsNoTracking()
                .Where(x => groupIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync()
        ).ToHashSet();

        var existingSubjectIds = (
            await _context.Subjects
                .AsNoTracking()
                .Where(x => subjectIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync()
        ).ToHashSet();

        var existingTeacherIds = (
            await _context.Teachers
                .AsNoTracking()
                .Where(x => teacherIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync()
        ).ToHashSet();

        var existingClassroomIds = (
            await _context.Classrooms
                .AsNoTracking()
                .Where(x => classroomIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync()
        ).ToHashSet();


        foreach (var item in items)
        {
            if (!existingGroupIds.Contains(item.GroupId))
            {
                TempData["ErrorMessage"] =
                    $"Группа «{item.GroupName}» больше не существует.";

                RemoveStoredPreview();

                return RedirectToAction(nameof(Index));
            }

            if (!existingSubjectIds.Contains(item.SubjectId))
            {
                TempData["ErrorMessage"] =
                    $"Предмет «{item.SubjectName}» больше не существует.";

                RemoveStoredPreview();

                return RedirectToAction(nameof(Index));
            }

            if (!existingTeacherIds.Contains(item.TeacherId))
            {
                TempData["ErrorMessage"] =
                    $"Преподаватель «{item.TeacherName}» больше не существует.";

                RemoveStoredPreview();

                return RedirectToAction(nameof(Index));
            }

            if (!existingClassroomIds.Contains(item.ClassroomId))
            {
                TempData["ErrorMessage"] =
                    $"Аудитория «{item.ClassroomName}» больше не существует.";

                RemoveStoredPreview();

                return RedirectToAction(nameof(Index));
            }

            if (item.StartTime >= item.EndTime)
            {
                TempData["ErrorMessage"] =
                    $"Некорректное время занятия «{item.SubjectName}».";

                RemoveStoredPreview();

                return RedirectToAction(nameof(Index));
            }
        }

        // --------------------------------------------------------
        // Проверяем конфликты внутри сгенерированного результата
        // --------------------------------------------------------

        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                var first = items[i];
                var second = items[j];

                if (first.DayOfWeek != second.DayOfWeek)
                {
                    continue;
                }

                var overlaps =
                    first.StartTime < second.EndTime &&
                    first.EndTime > second.StartTime;

                if (!overlaps)
                {
                    continue;
                }

                // Одна группа не может иметь два занятия одновременно.
                if (first.GroupId == second.GroupId)
                {
                    TempData["ErrorMessage"] =
                        $"Обнаружен конфликт группы «{first.GroupName}»: " +
                        $"{first.StartTime:hh\\:mm}–{first.EndTime:hh\\:mm}.";

                    RemoveStoredPreview();

                    return RedirectToAction(nameof(Index));
                }

                // Один преподаватель не может вести два занятия одновременно.
                if (first.TeacherId == second.TeacherId)
                {
                    TempData["ErrorMessage"] =
                        $"Обнаружен конфликт преподавателя " +
                        $"«{first.TeacherName}»: " +
                        $"{first.StartTime:hh\\:mm}–{first.EndTime:hh\\:mm}.";

                    RemoveStoredPreview();

                    return RedirectToAction(nameof(Index));
                }

                // Одна аудитория не может использоваться двумя группами.
                if (first.ClassroomId == second.ClassroomId)
                {
                    TempData["ErrorMessage"] =
                        $"Обнаружен конфликт аудитории " +
                        $"«{first.ClassroomName}»: " +
                        $"{first.StartTime:hh\\:mm}–{first.EndTime:hh\\:mm}.";

                    RemoveStoredPreview();

                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // --------------------------------------------------------
        // Получаем существующие записи БД
        // --------------------------------------------------------

        var days = items
            .Select(x => x.DayOfWeek)
            .Distinct()
            .ToList();

        var existingSchedules = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.DayOfWeek != null &&
                days.Contains(s.DayOfWeek.Value))
            .ToListAsync();

        // --------------------------------------------------------
        // Проверяем конфликты с уже существующим расписанием
        // --------------------------------------------------------

        foreach (var item in items)
        {
            foreach (var existing in existingSchedules)
            {
                if (existing.DayOfWeek != item.DayOfWeek)
                {
                    continue;
                }

                if (existing.StartTime == null ||
                    existing.EndTime == null)
                {
                    continue;
                }

                var overlaps =
                    existing.StartTime.Value < item.EndTime &&
                    existing.EndTime.Value > item.StartTime;

                if (!overlaps)
                {
                    continue;
                }

                // ------------------------------------------------
                // Конфликт группы
                // ------------------------------------------------

                if (existing.GroupId == item.GroupId)
                {
                    TempData["ErrorMessage"] =
                        $"Невозможно сохранить расписание. " +
                        $"У группы «{item.GroupName}» уже есть занятие " +
                        $"в это время.";

                    RemoveStoredPreview();

                    return RedirectToAction(nameof(Index));
                }

                // ------------------------------------------------
                // Конфликт преподавателя
                // ------------------------------------------------

                if (existing.TeacherId == item.TeacherId)
                {
                    TempData["ErrorMessage"] =
                        $"Невозможно сохранить расписание. " +
                        $"У преподавателя «{item.TeacherName}» уже есть " +
                        $"занятие в это время.";

                    RemoveStoredPreview();

                    return RedirectToAction(nameof(Index));
                }

                // ------------------------------------------------
                // Конфликт аудитории
                // ------------------------------------------------

                if (existing.ClassroomId == item.ClassroomId)
                {
                    TempData["ErrorMessage"] =
                        $"Невозможно сохранить расписание. " +
                        $"Аудитория «{item.ClassroomName}» уже занята " +
                        $"в это время.";

                    RemoveStoredPreview();

                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // --------------------------------------------------------
        // Сохраняем всё одной транзакцией
        // --------------------------------------------------------

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var schedules = items
                .Select(item => new Schedule
                {
                    GroupId = item.GroupId,
                    SubjectId = item.SubjectId,
                    TeacherId = item.TeacherId,
                    ClassroomId = item.ClassroomId,
                    DayOfWeek = item.DayOfWeek,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime
                })
                .ToList();

            await _context.Schedules.AddRangeAsync(schedules);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            RemoveStoredPreview();

            TempData["SuccessMessage"] =
                $"Расписание успешно сохранено. " +
                $"Добавлено занятий: {schedules.Count}.";

            return RedirectToAction(
                "Index",
                "Schedules"
            );
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            TempData["ErrorMessage"] =
                "При сохранении расписания произошла ошибка. " +
                "Изменения не были сохранены.";

            return RedirectToAction(nameof(Index));
        }
    }

    // ============================================================
    // GET GROUP LOAD
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetGroupLoad(int groupId)
    {
        var groupExists = await _context.Groups
            .AsNoTracking()
            .AnyAsync(g => g.Id == groupId);

        if (!groupExists)
        {
            return NotFound();
        }

        var load = await _context.GroupSubjects
            .AsNoTracking()
            .Where(gs => gs.GroupId == groupId)
            .Include(gs => gs.Subject)
                .ThenInclude(s =>
                    s!.ClassroomCategoryRequirements)
                    .ThenInclude(x => x.ClassroomCategory)
            .OrderBy(gs => gs.Subject!.Name)
            .Select(gs => new
            {
                subjectId = gs.SubjectId,

                subjectName = gs.Subject != null
                    ? gs.Subject.Name
                    : "Предмет не найден",

                weeklyLessons = gs.WeeklyLessons,

                type = gs.Subject != null
                    ? gs.Subject.Type
                    : SubjectType.General,

                classroomCategoryIds =
                    gs.Subject != null
                        ? gs.Subject
                            .ClassroomCategoryRequirements
                            .Select(x => x.ClassroomCategoryId)
                            .ToList()
                        : new List<int>(),

                classroomCategoryNames =
                    gs.Subject != null
                        ? gs.Subject
                            .ClassroomCategoryRequirements
                            .Where(x =>
                                x.ClassroomCategory != null)
                            .Select(x =>
                                x.ClassroomCategory!.Name)
                            .ToList()
                        : new List<string>()
            })
            .ToListAsync();

        return Json(load);
    }

    // ============================================================
    // LOAD VIEW DATA
    // ============================================================

    private async Task LoadViewDataAsync()
    {
        ViewBag.Groups = await _context.Groups
            .AsNoTracking()
            .OrderBy(g => g.Course)
            .ThenBy(g => g.Name)
            .ToListAsync();

        ViewBag.Subjects = await _context.Subjects
            .AsNoTracking()
            .Include(s =>
                s.ClassroomCategoryRequirements)
                .ThenInclude(x =>
                    x.ClassroomCategory)
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.GroupSubjects = await _context.GroupSubjects
            .AsNoTracking()
            .Include(gs => gs.Group)
            .Include(gs => gs.Subject)
                .ThenInclude(s =>
                    s!.ClassroomCategoryRequirements)
                    .ThenInclude(x =>
                        x.ClassroomCategory)
            .ToListAsync();

        ViewBag.ClassroomCategories =
            await _context.ClassroomCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private void ValidateGenerationRequest(
        ScheduleGenerationRequest model)
    {
        // --------------------------------------------------------
        // GROUPS
        // --------------------------------------------------------

        if (model.SelectedGroupIds == null ||
            model.SelectedGroupIds.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Выберите хотя бы одну группу."
            );
        }

        // --------------------------------------------------------
        // DAYS
        // --------------------------------------------------------

        if (model.Days == null ||
            model.Days.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Выберите хотя бы один день недели."
            );
        }

        // --------------------------------------------------------
        // TIME SLOTS
        // --------------------------------------------------------

        if (model.TimeSlots == null ||
            model.TimeSlots.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Добавьте хотя бы один временной интервал."
            );
        }
        else
        {
            ValidateTimeSlots(model.TimeSlots);
        }

        // --------------------------------------------------------
        // SELECTED GROUPS
        // --------------------------------------------------------

        if (model.SelectedGroupIds != null &&
            model.SelectedGroupIds.Count > 0)
        {
            var selectedGroupIds = model.SelectedGroupIds
                .Distinct()
                .ToList();

            var existingGroupIds = _context.Groups
                .AsNoTracking()
                .Where(g =>
                    selectedGroupIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToHashSet();

            var invalidGroups = selectedGroupIds
                .Where(id =>
                    !existingGroupIds.Contains(id))
                .ToList();

            if (invalidGroups.Count > 0)
            {
                ModelState.AddModelError(
                    "",
                    "Некоторые выбранные группы не существуют."
                );
            }
        }

        // --------------------------------------------------------
        // LIMITS
        // --------------------------------------------------------

        if (model.MaxLessonsPerDay < 1 ||
            model.MaxLessonsPerDay > 10)
        {
            ModelState.AddModelError(
                "",
                "Максимальное количество пар в день " +
                "должно быть от 1 до 10."
            );
        }

        if (model.MaxConsecutiveLessons < 1 ||
            model.MaxConsecutiveLessons > 10)
        {
            ModelState.AddModelError(
                "",
                "Максимальное количество пар подряд " +
                "должно быть от 1 до 10."
            );
        }
    }

    // ============================================================
    // TIME SLOT VALIDATION
    // ============================================================

    private void ValidateTimeSlots(
        List<GenerationTimeSlot> slots)
    {
        // --------------------------------------------------------
        // INVALID INTERVALS
        // --------------------------------------------------------

        for (var i = 0; i < slots.Count; i++)
        {
            var current = slots[i];

            if (current.StartTime >= current.EndTime)
            {
                ModelState.AddModelError(
                    "",
                    $"Некорректный интервал №{i + 1}: " +
                    $"{current.StartTime:hh\\:mm} — " +
                    $"{current.EndTime:hh\\:mm}."
                );
            }
        }

        // --------------------------------------------------------
        // OVERLAPPING INTERVALS
        // --------------------------------------------------------

        for (var i = 0; i < slots.Count; i++)
        {
            for (var j = i + 1;
                 j < slots.Count;
                 j++)
            {
                var first = slots[i];
                var second = slots[j];

                var overlaps =
                    first.StartTime < second.EndTime &&
                    first.EndTime > second.StartTime;

                if (overlaps)
                {
                    ModelState.AddModelError(
                        "",
                        $"Временные интервалы №{i + 1} " +
                        $"и №{j + 1} пересекаются."
                    );
                }
            }
        }
    }

    // ============================================================
    // PREVIEW STORAGE
    // ============================================================

    private void StorePreview(
        ScheduleGenerationPreviewViewModel preview)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(
            preview,
            options
        );

        TempData[PreviewDataKey] = json;
    }

    // ============================================================
    // GET STORED PREVIEW
    // ============================================================

    private ScheduleGenerationPreviewViewModel?
        GetStoredPreview()
    {
        var json = TempData[PreviewDataKey] as string;

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize
                <ScheduleGenerationPreviewViewModel>(
                    json,
                    options
                );
        }
        catch
        {
            return null;
        }
    }

    // ============================================================
    // REMOVE STORED PREVIEW
    // ============================================================

    private void RemoveStoredPreview()
    {
        TempData.Remove(PreviewDataKey);
    }
}

