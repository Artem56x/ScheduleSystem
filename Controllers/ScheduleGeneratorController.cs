using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;

    private const string PreviewSessionKey =
        "ScheduleGeneratorPreviewId";

    private static readonly TimeSpan PreviewLifetime =
        TimeSpan.FromMinutes(20);

    public ScheduleGeneratorController(
        ApplicationDbContext context,
        ScheduleGeneratorService generatorService,
        IMemoryCache cache)
    {
        _context = context;
        _generatorService = generatorService;
        _cache = cache;
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
                }
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

        NormalizeRequest(model);

        var validationErrors =
            ValidateRequest(model);

        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError("", error);
            }

            return View("Index", model);
        }

        var result =
            await _generatorService.GenerateAsync(model);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View("Index", model);
        }

        var groupIds = model.SelectedGroupIds
            .Distinct()
            .ToList();

        var days = model.Days
            .Distinct()
            .ToList();

        var existingCount =
            await _context.Schedules
                .AsNoTracking()
                .CountAsync(s =>
                    s.DayOfWeek.HasValue &&
                    days.Contains(s.DayOfWeek.Value) &&
                    groupIds.Contains(s.GroupId));

        var preview =
            new ScheduleGenerationPreviewViewModel
            {
                Items = result.GeneratedItems,

                SelectedGroupIds = groupIds,

                SelectedDays = days,

                TimeSlots = model.TimeSlots
                    .OrderBy(x => x.StartTime)
                    .ToList(),

                MaxLessonsPerDay =
                    model.MaxLessonsPerDay,

                MaxConsecutiveLessons =
                    model.MaxConsecutiveLessons,

                DistributeLessons =
                    model.DistributeLessons,

                UseClassroomRecommendations =
                    model.UseClassroomRecommendations,

                ExistingSchedulesToReplace =
                    existingCount
            };

        StorePreview(preview);

        return View("Preview", preview);
    }

    // ============================================================
    // PREVIEW
    // ============================================================

    [HttpGet]
    public IActionResult Preview()
    {
        var preview = GetStoredPreview();

        if (preview == null)
        {
            TempData["ErrorMessage"] =
                "Предпросмотр расписания истёк. " +
                "Выполните генерацию заново.";

            return RedirectToAction(nameof(Index));
        }

        return View(preview);
    }

    // ============================================================
    // CONFIRM AND SAVE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAndSave()
    {
        var preview = GetStoredPreview();

        if (preview == null ||
            preview.Items.Count == 0)
        {
            TempData["ErrorMessage"] =
                "Предпросмотр расписания не найден. " +
                "Сначала выполните генерацию.";

            return RedirectToAction(nameof(Index));
        }

        // ========================================================
        // TRANSACTION
        // ========================================================

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

        try
        {
            // ----------------------------------------------------
            // FINAL VALIDATION
            // ----------------------------------------------------

            var validationErrors =
                await _generatorService.ValidateBeforeSaveAsync(
                    preview);

            if (validationErrors.Count > 0)
            {
                await transaction.RollbackAsync();

                TempData["ErrorMessage"] =
                    "Расписание изменилось после генерации. " +
                    "Предпросмотр больше нельзя безопасно сохранить.";

                TempData["GenerationValidationErrors"] =
                    string.Join(
                        "\n",
                        validationErrors);

                return RedirectToAction(nameof(Preview));
            }

            var groupIds = preview.SelectedGroupIds
                .Distinct()
                .ToList();

            var days = preview.SelectedDays
                .Distinct()
                .ToList();

            // ----------------------------------------------------
            // DELETE OLD SCHEDULE
            // ----------------------------------------------------

            var oldSchedules =
                await _context.Schedules
                    .Where(s =>
                        groupIds.Contains(s.GroupId) &&
                        s.DayOfWeek.HasValue &&
                        days.Contains(s.DayOfWeek.Value))
                    .ToListAsync();

            if (oldSchedules.Count > 0)
            {
                _context.Schedules.RemoveRange(
                    oldSchedules);

                await _context.SaveChangesAsync();
            }

            // ----------------------------------------------------
            // INSERT NEW SCHEDULE
            // ----------------------------------------------------

            var newSchedules =
                preview.Items
                    .Select(item =>
                        new Schedule
                        {
                            GroupId =
                                item.GroupId,

                            SubjectId =
                                item.SubjectId,

                            TeacherId =
                                item.TeacherId,

                            ClassroomId =
                                item.ClassroomId,

                            DayOfWeek =
                                item.DayOfWeek,

                            StartTime =
                                item.StartTime,

                            EndTime =
                                item.EndTime
                        })
                    .ToList();

            await _context.Schedules
                .AddRangeAsync(newSchedules);

            await _context.SaveChangesAsync();

            // ----------------------------------------------------
            // COMMIT
            // ----------------------------------------------------

            await transaction.CommitAsync();

            RemoveStoredPreview();

            TempData["SuccessMessage"] =
                $"Расписание успешно заменено. " +
                $"Добавлено занятий: {newSchedules.Count}.";

            return RedirectToAction(
                "Index",
                "Schedules");
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["ErrorMessage"] =
                "При сохранении расписания произошла ошибка. " +
                "Старое расписание осталось без изменений.";

            return RedirectToAction(nameof(Preview));
        }
    }

    // ============================================================
    // CANCEL PREVIEW
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CancelPreview()
    {
        RemoveStoredPreview();

        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // GET GROUP LOAD
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetGroupLoad(
        int groupId)
    {
        var groupExists =
            await _context.Groups
                .AsNoTracking()
                .AnyAsync(g => g.Id == groupId);

        if (!groupExists)
        {
            return NotFound();
        }

        var load =
            await _context.GroupSubjects
                .AsNoTracking()
                .Where(gs =>
                    gs.GroupId == groupId)
                .Include(gs => gs.Subject)
                    .ThenInclude(s =>
                        s!.ClassroomCategoryRequirements)
                    .ThenInclude(x =>
                        x.ClassroomCategory)
                .OrderBy(gs =>
                    gs.Subject!.Name)
                .Select(gs => new
                {
                    subjectId = gs.SubjectId,

                    subjectName =
                        gs.Subject != null
                            ? gs.Subject.Name
                            : "Предмет не найден",

                    weeklyLessons =
                        gs.WeeklyLessons,

                    type =
                        gs.Subject != null
                            ? gs.Subject.Type
                            : SubjectType.General,

                    classroomCategoryIds =
                        gs.Subject != null
                            ? gs.Subject
                                .ClassroomCategoryRequirements
                                .Select(x =>
                                    x.ClassroomCategoryId)
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
    // VIEW DATA
    // ============================================================

    private async Task LoadViewDataAsync()
    {
        ViewBag.Groups =
            await _context.Groups
                .AsNoTracking()
                .OrderBy(g => g.Course)
                .ThenBy(g => g.Name)
                .ToListAsync();

        ViewBag.Subjects =
            await _context.Subjects
                .AsNoTracking()
                .Include(s =>
                    s.ClassroomCategoryRequirements)
                .ThenInclude(x =>
                    x.ClassroomCategory)
                .OrderBy(s => s.Name)
                .ToListAsync();

        ViewBag.GroupSubjects =
            await _context.GroupSubjects
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
    // REQUEST NORMALIZATION
    // ============================================================

    private static void NormalizeRequest(
        ScheduleGenerationRequest model)
    {
        model.SelectedGroupIds ??= new List<int>();
        model.Days ??= new List<DayOfWeek>();
        model.TimeSlots ??= new List<GenerationTimeSlot>();

        model.SelectedGroupIds = model.SelectedGroupIds
            .Distinct()
            .ToList();

        model.Days = model.Days
            .Distinct()
            .ToList();

        model.TimeSlots = model.TimeSlots
            .OrderBy(x => x?.StartTime ?? TimeSpan.MinValue)
            .ToList();
    }

    // ============================================================
    // REQUEST VALIDATION
    // ============================================================

    private List<string> ValidateRequest(
        ScheduleGenerationRequest model)
    {
        var errors = new List<string>();

        if (model.SelectedGroupIds == null ||
            model.SelectedGroupIds.Count == 0)
        {
            errors.Add("Выберите хотя бы одну группу.");
        }
        else if (model.SelectedGroupIds.Any(id => id <= 0))
        {
            errors.Add("Выбраны некорректные идентификаторы групп.");
        }

        if (model.Days == null || model.Days.Count == 0)
        {
            errors.Add("Выберите хотя бы один день недели.");
        }
        else if (model.Days.Any(day => !Enum.IsDefined(typeof(DayOfWeek), day)))
        {
            errors.Add("Выбрано некорректное значение дня недели.");
        }

        if (model.TimeSlots == null || model.TimeSlots.Count == 0)
        {
            errors.Add("Добавьте хотя бы один временной интервал.");
        }
        else
        {
            var dayLength = TimeSpan.FromDays(1);

            for (var i = 0; i < model.TimeSlots.Count; i++)
            {
                var slot = model.TimeSlots[i];

                if (slot == null)
                {
                    errors.Add($"Интервал №{i + 1} не заполнен.");
                    continue;
                }

                if (slot.StartTime < TimeSpan.Zero ||
                    slot.StartTime >= dayLength ||
                    slot.EndTime <= TimeSpan.Zero ||
                    slot.EndTime > dayLength ||
                    slot.StartTime >= slot.EndTime)
                {
                    errors.Add(
                        $"Интервал №{i + 1} имеет некорректное время. " +
                        "Начало должно быть раньше окончания, " +
                        "а время должно находиться в пределах суток.");
                }
            }

            for (var i = 0; i < model.TimeSlots.Count; i++)
            {
                var first = model.TimeSlots[i];

                if (first == null ||
                    first.StartTime < TimeSpan.Zero ||
                    first.StartTime >= dayLength ||
                    first.EndTime <= TimeSpan.Zero ||
                    first.EndTime > dayLength ||
                    first.StartTime >= first.EndTime)
                {
                    continue;
                }

                for (var j = i + 1; j < model.TimeSlots.Count; j++)
                {
                    var second = model.TimeSlots[j];

                    if (second == null ||
                        second.StartTime < TimeSpan.Zero ||
                        second.StartTime >= dayLength ||
                        second.EndTime <= TimeSpan.Zero ||
                        second.EndTime > dayLength ||
                        second.StartTime >= second.EndTime)
                    {
                        continue;
                    }

                    if (first.StartTime < second.EndTime &&
                        first.EndTime > second.StartTime)
                    {
                        errors.Add(
                            $"Интервалы №{i + 1} и №{j + 1} пересекаются.");
                    }
                }
            }
        }

        if (model.MaxLessonsPerDay < 1 ||
            model.MaxLessonsPerDay > 10)
        {
            errors.Add(
                "Максимум пар в день должен быть от 1 до 10.");
        }

        if (model.MaxConsecutiveLessons < 1 ||
            model.MaxConsecutiveLessons > 10)
        {
            errors.Add(
                "Максимум пар подряд должен быть от 1 до 10.");
        }

        return errors;
    }

    // ============================================================
    // PREVIEW CACHE
    // ============================================================

    private void StorePreview(
        ScheduleGenerationPreviewViewModel preview)
    {
        var previewId =
            Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

        _cache.Set(
            GetCacheKey(previewId),
            preview,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    PreviewLifetime,

                SlidingExpiration =
                    TimeSpan.FromMinutes(10)
            });

        HttpContext.Session.SetString(
            PreviewSessionKey,
            previewId);
    }

    private ScheduleGenerationPreviewViewModel?
        GetStoredPreview()
    {
        var previewId =
            HttpContext.Session.GetString(
                PreviewSessionKey);

        if (string.IsNullOrWhiteSpace(previewId))
        {
            return null;
        }

        return _cache.Get<ScheduleGenerationPreviewViewModel>(
            GetCacheKey(previewId));
    }

    private void RemoveStoredPreview()
    {
        var previewId =
            HttpContext.Session.GetString(
                PreviewSessionKey);

        if (!string.IsNullOrWhiteSpace(previewId))
        {
            _cache.Remove(
                GetCacheKey(previewId));
        }

        HttpContext.Session.Remove(
            PreviewSessionKey);
    }

    private static string GetCacheKey(
        string previewId)
    {
        return $"ScheduleGenerator:Preview:{previewId}";
    }
}