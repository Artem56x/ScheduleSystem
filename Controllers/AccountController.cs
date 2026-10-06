using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }


    // ============================================================
    // CLEAR SCHEDULE
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearSchedule()
    {
        try
        {
            await _context.Schedules.ExecuteDeleteAsync();

            TempData["SuccessMessage"] =
                "Расписание успешно очищено.";
        }
        catch
        {
            TempData["ErrorMessage"] =
                "Не удалось очистить расписание.";
        }

        return RedirectToAction(nameof(Settings));
    }


    // ============================================================
    // CLEAR ALL PROGRAM DATA
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAllData()
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // ----------------------------------------------------
            // 1. Расписание
            // ----------------------------------------------------

            await _context.Schedules
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // 2. Учебная нагрузка групп
            // ----------------------------------------------------

            await _context.GroupSubjects
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // 3. Требования предметов к категориям аудиторий
            // ----------------------------------------------------

            await _context.SubjectClassroomCategories
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // 4. Преподаватели
            // ----------------------------------------------------

            await _context.Teachers
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // 5. Предметы
            // ----------------------------------------------------

            await _context.Subjects
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // 6. Группы
            // ----------------------------------------------------

            await _context.Groups
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // 7. Аудитории
            // ----------------------------------------------------

            await _context.Classrooms
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // 8. Категории аудиторий
            // ----------------------------------------------------

            await _context.ClassroomCategories
                .ExecuteDeleteAsync();


            // ----------------------------------------------------
            // Commit
            // ----------------------------------------------------

            await transaction.CommitAsync();

            TempData["SuccessMessage"] =
                "Все данные программы успешно удалены.";
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["ErrorMessage"] =
                "Не удалось удалить данные программы. " +
                "Изменения отменены.";
        }

        return RedirectToAction(nameof(Settings));
    }


    // ============================================================
    // LOGIN
    // ============================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        return View();
    }


    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string? email,
        string? password,
        bool rememberMe = false,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(
                "",
                "Введите email.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                "",
                "Введите пароль.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }
        email = email!.Trim();
        password = password!.Trim();

        var result = await _signInManager.PasswordSignInAsync(
            email,
            password,
            rememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;

                user.LastLoginDevice = GetDeviceName(
                    Request.Headers.UserAgent.ToString()
                );

                await _userManager.UpdateAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                "",
                "Учётная запись временно заблокирована.");

            return View();
        }

        ModelState.AddModelError(
            "",
            "Неверный email или пароль.");

        return View();
    }

    // ============================================================
    // REGISTER
    // ============================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }


    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string? email,
        string? password,
        string? displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(
                "",
                "Введите email.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                "",
                "Введите пароль.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        email = email!.Trim();
        password = password!.Trim();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName =
                displayName?.Trim() ??
                string.Empty
        };

        var result =
            await _userManager.CreateAsync(
                user,
                password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(
                user,
                "User");

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return RedirectToAction(
                "Index",
                "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                "",
                error.Description);
        }

        return View();
    }


    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        string? currentPassword,
        string? newPassword,
        string? confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) ||
            string.IsNullOrWhiteSpace(newPassword) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            TempData["ErrorMessage"] =
                "Заполните все поля.";

            return RedirectToAction(
                nameof(Settings));
        }

        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] =
                "Новые пароли не совпадают.";

            return RedirectToAction(
                nameof(Settings));
        }

        var user =
            await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var result =
            await _userManager.ChangePasswordAsync(
                user,
                currentPassword,
                newPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(
                user);

            TempData["SuccessMessage"] =
                "Пароль успешно изменён.";

            return RedirectToAction(
                nameof(Settings));
        }

        TempData["ErrorMessage"] =
            string.Join(
                " ",
                result.Errors.Select(
                    error => error.Description));

        return RedirectToAction(
            nameof(Settings));
    }


    // ============================================================
    // LOGOUT
    // ============================================================

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(
            "Index",
            "Home");
    }


    // ============================================================
    // PROFILE
    // ============================================================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        LoadUserViewData(user);

        return View();
    }


    // ============================================================
    // UPDATE PROFILE
    // ============================================================

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        string? displayName,
        string? email)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        displayName = displayName?.Trim();
        email = email?.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            TempData["ErrorMessage"] =
                "Введите имя пользователя.";

            return RedirectToAction(nameof(Settings));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] =
                "Введите адрес электронной почты.";

            return RedirectToAction(nameof(Settings));
        }

        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                .IsValid(email))
        {
            TempData["ErrorMessage"] =
                "Введите корректный адрес электронной почты.";

            return RedirectToAction(nameof(Settings));
        }

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null && existingUser.Id != user.Id)
        {
            TempData["ErrorMessage"] =
                "Этот адрес электронной почты уже используется.";

            return RedirectToAction(nameof(Settings));
        }

        user.DisplayName = displayName;

        var emailResult =
            await _userManager.SetEmailAsync(user, email);

        if (!emailResult.Succeeded)
        {
            TempData["ErrorMessage"] =
                string.Join(" ", emailResult.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Settings));
        }

        var userNameResult =
            await _userManager.SetUserNameAsync(user, email);

        if (!userNameResult.Succeeded)
        {
            TempData["ErrorMessage"] =
                string.Join(" ", userNameResult.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Settings));
        }

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            TempData["ErrorMessage"] =
                string.Join(" ", updateResult.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Settings));
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["SuccessMessage"] =
            "Профиль успешно сохранён.";

        return RedirectToAction(nameof(Settings));
    }


    // ============================================================
    // SETTINGS
    // ============================================================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        LoadUserViewData(user);


        // --------------------------------------------------------
        // Данные учебной нагрузки нужны только администратору.
        // Настройки генерации здесь НЕ загружаются.
        // --------------------------------------------------------

        if (User.IsInRole("Admin"))
        {
            await LoadStudyLoadDataAsync();
        }

        return View();
    }

    // ============================================================
    // STUDY LOAD
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> StudyLoad()
    {
        await LoadStudyLoadDataAsync();

        return View();
    }

    // ============================================================
    // USER VIEW DATA
    // ============================================================
    private void LoadUserViewData(ApplicationUser user)
    {
        ViewBag.Email = user.Email ?? "";
        ViewBag.UserName = user.UserName ?? "";
        ViewBag.DisplayName = user.DisplayName ?? "Пользователь";
        ViewBag.IsAdmin = User.IsInRole("Admin");

        ViewBag.LastLoginAt = user.LastLoginAt;
        ViewBag.LastLoginDevice = user.LastLoginDevice
            ?? "Неизвестное устройство";

        ViewBag.AccountStatus = User.Identity?.IsAuthenticated == true
            ? "Активен"
            : "Неактивен";

        ViewBag.ScheduleNotificationsEnabled =
            user.ScheduleNotificationsEnabled;

        ViewBag.SystemNotificationsEnabled =
            user.SystemNotificationsEnabled;

    }


    // ============================================================
    // LOAD STUDY LOAD DATA
    // ============================================================

    private async Task LoadStudyLoadDataAsync()
    {
        var groups =
            await _context.Groups
                .AsNoTracking()
                .OrderBy(g => g.Course)
                .ThenBy(g => g.Name)
                .ToListAsync();


        var subjects =
            await _context.Subjects
                .AsNoTracking()
                .OrderBy(s => s.Course)
                .ThenBy(s => s.Name)
                .ToListAsync();


        var groupSubjects =
            await _context.GroupSubjects
                .AsNoTracking()
                .OrderBy(gs => gs.GroupId)
                .ThenBy(gs => gs.SubjectId)
                .ToListAsync();


        ViewBag.Groups =
            groups;

        ViewBag.Subjects =
            subjects;

        ViewBag.GroupSubjects =
            groupSubjects;
    }


    // ============================================================
    // ADD GROUP SUBJECT
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGroupSubject(
        int groupId,
        int subjectId,
        int weeklyLessons)
    {
        if (weeklyLessons < 1 ||
            weeklyLessons > 20)
        {
            TempData["ErrorMessage"] =
                "Количество занятий должно быть от 1 до 20.";

            return RedirectToAction(
                nameof(Settings));
        }


        var group =
            await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    g => g.Id == groupId);

        if (group is null)
        {
            TempData["ErrorMessage"] =
                "Выбранная группа не найдена.";

            return RedirectToAction(
                nameof(Settings));
        }


        var subject =
            await _context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.Id == subjectId);

        if (subject is null)
        {
            TempData["ErrorMessage"] =
                "Выбранный предмет не найден.";

            return RedirectToAction(
                nameof(Settings));
        }


        if (group.Course != subject.Course)
        {
            TempData["ErrorMessage"] =
                $"Нельзя добавить предмет «{subject.Name}» " +
                $"({subject.Course} курс) группе «{group.Name}» " +
                $"({group.Course} курс). " +
                "Предмет и группа должны быть одного курса.";

            return RedirectToAction(
                nameof(Settings));
        }


        var exists =
            await _context.GroupSubjects
                .AnyAsync(
                    gs =>
                        gs.GroupId == groupId &&
                        gs.SubjectId == subjectId);

        if (exists)
        {
            TempData["ErrorMessage"] =
                "Этот предмет уже добавлен " +
                "в учебную нагрузку группы.";

            return RedirectToAction(
                nameof(Settings));
        }


        var groupSubject =
            new GroupSubject
            {
                GroupId = groupId,
                SubjectId = subjectId,
                WeeklyLessons = weeklyLessons
            };

        _context.GroupSubjects.Add(
            groupSubject);

        await _context.SaveChangesAsync();


        TempData["SuccessMessage"] =
            $"Предмет «{subject.Name}» добавлен " +
            $"в нагрузку группы «{group.Name}».";


        return RedirectToAction(
            nameof(Settings));
    }


    // ============================================================
    // SAVE GROUP SUBJECTS
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGroupSubjects(
        int groupId,
        List<StudyLoadSaveItem>? items)
    {
        items ??= new List<StudyLoadSaveItem>();

        // --------------------------------------------------------
        // GROUP
        // --------------------------------------------------------

        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group is null)
        {
            TempData["ErrorMessage"] =
                "Выбранная группа не найдена.";

            return RedirectToAction(nameof(Settings));
        }


        // --------------------------------------------------------
        // EMPTY LOAD
        // --------------------------------------------------------

        // Пустой список означает:
        // пользователь удалил все предметы группы.
        if (items.Count == 0)
        {
            await using var emptyTransaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var existingItems = await _context.GroupSubjects
                    .Where(gs => gs.GroupId == groupId)
                    .ToListAsync();

                if (existingItems.Count > 0)
                {
                    _context.GroupSubjects.RemoveRange(existingItems);

                    await _context.SaveChangesAsync();
                }

                await emptyTransaction.CommitAsync();

                TempData["SuccessMessage"] =
                    $"Учебная нагрузка группы «{group.Name}» сохранена.";

                return RedirectToAction(nameof(Settings));
            }
            catch
            {
                await emptyTransaction.RollbackAsync();

                TempData["ErrorMessage"] =
                    "Не удалось сохранить учебную нагрузку.";

                return RedirectToAction(nameof(Settings));
            }
        }


        // --------------------------------------------------------
        // LIMIT
        // --------------------------------------------------------

        if (items.Count > 100)
        {
            TempData["ErrorMessage"] =
                "Слишком много предметов в учебной нагрузке.";

            return RedirectToAction(nameof(Settings));
        }


        // --------------------------------------------------------
        // VALIDATE DUPLICATES
        // --------------------------------------------------------

        var subjectIds = new HashSet<int>();

        foreach (var item in items)
        {
            if (item.SubjectId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Некорректный предмет в учебной нагрузке.";

                return RedirectToAction(nameof(Settings));
            }

            if (!subjectIds.Add(item.SubjectId))
            {
                TempData["ErrorMessage"] =
                    "Один и тот же предмет нельзя добавить " +
                    "в учебную нагрузку несколько раз.";

                return RedirectToAction(nameof(Settings));
            }

            if (item.WeeklyLessons < 1 ||
                item.WeeklyLessons > 20)
            {
                TempData["ErrorMessage"] =
                    "Количество занятий должно быть от 1 до 20.";

                return RedirectToAction(nameof(Settings));
            }
        }


        // --------------------------------------------------------
        // LOAD SUBJECTS
        // --------------------------------------------------------

        var subjects = await _context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        if (subjects.Count != subjectIds.Count)
        {
            TempData["ErrorMessage"] =
                "Один или несколько выбранных предметов не найдены.";

            return RedirectToAction(nameof(Settings));
        }


        // --------------------------------------------------------
        // VALIDATE COURSE
        // --------------------------------------------------------

        foreach (var item in items)
        {
            var subject = subjects[item.SubjectId];

            if (subject.Course != group.Course)
            {
                TempData["ErrorMessage"] =
                    $"Нельзя добавить предмет «{subject.Name}» " +
                    $"({subject.Course} курс) группе «{group.Name}» " +
                    $"({group.Course} курс). " +
                    "Предмет и группа должны быть одного курса.";

                return RedirectToAction(nameof(Settings));
            }
        }


        // --------------------------------------------------------
        // TRANSACTION
        // --------------------------------------------------------

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // ----------------------------------------------------
            // EXISTING GROUP SUBJECTS
            // ----------------------------------------------------

            var existingItems = await _context.GroupSubjects
                .Where(gs => gs.GroupId == groupId)
                .ToListAsync();


            var existingById = existingItems
                .ToDictionary(gs => gs.Id);


            var existingBySubjectId = existingItems
                .ToDictionary(gs => gs.SubjectId);


            // ----------------------------------------------------
            // IDs THAT REMAIN
            // ----------------------------------------------------

            var postedExistingIds = new HashSet<int>();


            // ----------------------------------------------------
            // INSERT / UPDATE
            // ----------------------------------------------------

            foreach (var item in items)
            {
                // ------------------------------------------------
                // EXISTING RECORD
                // ------------------------------------------------

                if (item.Id.HasValue &&
                    existingById.TryGetValue(
                        item.Id.Value,
                        out var existingByIdItem))
                {
                    existingByIdItem.WeeklyLessons =
                        item.WeeklyLessons;

                    postedExistingIds.Add(
                        existingByIdItem.Id);

                    continue;
                }


                // ------------------------------------------------
                // EXISTING RECORD WITHOUT ID
                // ------------------------------------------------

                if (existingBySubjectId.TryGetValue(
                        item.SubjectId,
                        out var existingBySubjectItem))
                {
                    existingBySubjectItem.WeeklyLessons =
                        item.WeeklyLessons;

                    postedExistingIds.Add(
                        existingBySubjectItem.Id);

                    continue;
                }


                // ------------------------------------------------
                // NEW RECORD
                // ------------------------------------------------

                var newGroupSubject = new GroupSubject
                {
                    GroupId = groupId,
                    SubjectId = item.SubjectId,
                    WeeklyLessons = item.WeeklyLessons
                };

                _context.GroupSubjects.Add(
                    newGroupSubject);
            }


            // ----------------------------------------------------
            // DELETE REMOVED SUBJECTS
            // ----------------------------------------------------

            var itemsToDelete = existingItems
                .Where(gs =>
                    !postedExistingIds.Contains(gs.Id) &&
                    !items.Any(item =>
                        item.Id == gs.Id ||
                        item.SubjectId == gs.SubjectId))
                .ToList();


            if (itemsToDelete.Count > 0)
            {
                _context.GroupSubjects.RemoveRange(
                    itemsToDelete);
            }


            // ----------------------------------------------------
            // SAVE
            // ----------------------------------------------------

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();


            TempData["SuccessMessage"] =
                $"Учебная нагрузка группы «{group.Name}» сохранена.";
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["ErrorMessage"] =
                "Не удалось сохранить учебную нагрузку.";
        }


        return RedirectToAction(nameof(Settings));
    }


    // ============================================================
    // STUDY LOAD SAVE MODEL
    // ============================================================

    public class StudyLoadSaveItem
    {
        public int? Id { get; set; }

        public int SubjectId { get; set; }

        public int WeeklyLessons { get; set; }
    }

    private static string GetDeviceName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return "Неизвестное устройство";
        }

        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
        {
            return "Windows";
        }

        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
        {
            return "Android";
        }

        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
        {
            return "iOS";
        }

        if (userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase))
        {
            return "macOS";
        }

        if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
        {
            return "Linux";
        }

        return "Неизвестное устройство";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotifications(
        bool scheduleNotificationsEnabled,
        bool systemNotificationsEnabled)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        user.ScheduleNotificationsEnabled = scheduleNotificationsEnabled;
        user.SystemNotificationsEnabled = systemNotificationsEnabled;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest();
        }

        return Ok(new
        {
            scheduleNotificationsEnabled = user.ScheduleNotificationsEnabled,
            systemNotificationsEnabled = user.SystemNotificationsEnabled
        });
    }

}