using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Models;

namespace ScheduleSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;


        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // =========================================================
        // CLEAR SCHEDULE
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearSchedule()
        {
            var schedules =
                await _context.Schedules.ToListAsync();

            if (schedules.Count == 0)
            {
                TempData["ProgramMessage"] =
                    "Расписание уже пустое.";

                return RedirectToAction(
                   nameof(Settings));
            }

            _context.Schedules.RemoveRange(schedules);

            await _context.SaveChangesAsync();

            TempData["ProgramMessage"] =
                $"Расписание успешно очищено. " +
                $"Удалено занятий: {schedules.Count}.";

            return RedirectToAction(
                nameof(Settings));
        }

        // =========================================================
        // CLEAR ALL DATA
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAllData()
        {
            var schedules =
                await _context.Schedules.ToListAsync();

            var groupSubjects =
                await _context.GroupSubjects.ToListAsync();

            var teachers =
                await _context.Teachers.ToListAsync();

            var groups =
                await _context.Groups.ToListAsync();

            var subjects =
                await _context.Subjects.ToListAsync();

            var classrooms =
                await _context.Classrooms.ToListAsync();

            var classroomCategories =
                await _context.ClassroomCategories.ToListAsync();

            var totalDeleted =
                schedules.Count +
                groupSubjects.Count +
                teachers.Count +
                groups.Count +
                subjects.Count +
                classrooms.Count +
                classroomCategories.Count;

            if (totalDeleted == 0)
            {
                TempData["ProgramMessage"] =
                    "Данные системы уже очищены.";

                return RedirectToAction(
                    nameof(Settings));
            }

            _context.Schedules.RemoveRange(schedules);
            _context.GroupSubjects.RemoveRange(groupSubjects);
            _context.Teachers.RemoveRange(teachers);
            _context.Groups.RemoveRange(groups);
            _context.Classrooms.RemoveRange(classrooms);
            _context.Subjects.RemoveRange(subjects);
            _context.ClassroomCategories.RemoveRange(classroomCategories);

            await _context.SaveChangesAsync();

            TempData["ProgramMessage"] =
                $"Все данные системы успешно сброшены. " +
                $"Удалено записей: {totalDeleted}.";

            return RedirectToAction(
                nameof(Settings));
        }

        // =========================================================
        // LOGIN
        // =========================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Введите email и пароль.");

                return View();
            }

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Неверный email или пароль.");

                return View();
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    password,
                    isPersistent: false,
                    lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            ModelState.AddModelError(
                "",
                "Неверный email или пароль.");

            return View();
        }

        // =========================================================
        // CHANGE PASSWORD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(
                    "",
                    "Заполните все поля.");

                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(
                    "",
                    "Новые пароли не совпадают.");

                return View();
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError(
                    "",
                    "Новый пароль должен содержать минимум 6 символов.");

                return View();
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result =
                await _userManager.ChangePasswordAsync(
                    user,
                    currentPassword,
                    newPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View();
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] =
                "Пароль успешно изменён.";

            return RedirectToAction(nameof(Settings));
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }

        // =========================================================
        // REGISTER
        // =========================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string email,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(
                    "",
                    "Заполните все поля.");

                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(
                    "",
                    "Пароли не совпадают.");

                return View();
            }

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "Пользователь с таким email уже существует.");

                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View();
            }

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

        // =========================================================
        // PROFILE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.Email = user.Email;
            ViewBag.DisplayName = user.DisplayName;

            ViewBag.IsAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin");

            return View();
        }

        // =========================================================
        // SETTINGS
        // =========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            ViewBag.Email =
                user.Email ?? "";

            ViewBag.UserName =
                user.UserName ??
                user.Email ??
                "";

            ViewBag.DisplayName =
                user.DisplayName ?? "";

            ViewBag.IsAdmin =
                User.IsInRole("Admin");

            if (User.IsInRole("Admin"))
            {
                await LoadProgramSettingsDataAsync();
            }

            return View();
        }

        // =========================================================
        // LOAD PROGRAM SETTINGS DATA
        // =========================================================

        private async Task LoadProgramSettingsDataAsync()
        {
            ViewBag.TeacherCount =
                await _context.Teachers.CountAsync();

            ViewBag.GroupCount =
                await _context.Groups.CountAsync();

            ViewBag.SubjectCount =
                await _context.Subjects.CountAsync();

            ViewBag.ClassroomCount =
                await _context.Classrooms.CountAsync();

            ViewBag.ScheduleCount =
                await _context.Schedules.CountAsync();

            var generationSettings =
                await _context.ScheduleGenerationSettings
                    .FirstOrDefaultAsync();

            if (generationSettings == null)
            {
                generationSettings =
                    new ScheduleGenerationSettings();

                _context.ScheduleGenerationSettings.Add(
                    generationSettings);

                await _context.SaveChangesAsync();
            }

            ViewBag.GenerationSettings =
                generationSettings;

            ViewBag.Groups =
                await _context.Groups
                    .AsNoTracking()
                    .OrderBy(g => g.Course)
                    .ThenBy(g => g.Name)
                    .ToListAsync();

            ViewBag.Subjects =
                await _context.Subjects
                    .AsNoTracking()
                    .OrderBy(s => s.Name)
                    .ToListAsync();

            ViewBag.GroupSubjects =
                await _context.GroupSubjects
                    .AsNoTracking()
                    .Include(gs => gs.Group)
                    .Include(gs => gs.Subject)
                    .OrderBy(gs => gs.Group!.Course)
                    .ThenBy(gs => gs.Group!.Name)
                    .ThenBy(gs => gs.Subject!.Name)
                    .ToListAsync();
        }

        // =========================================================
        // UPDATE GENERATION SETTINGS
        // =========================================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGenerationSettings(
            ScheduleGenerationSettings model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ProgramMessage"] =
                    "Не удалось сохранить настройки автогенерации. " +
                    "Проверьте введённые значения.";

                return RedirectToAction(nameof(Settings));
            }

            var settings =
                await _context.ScheduleGenerationSettings
                    .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings =
                    new ScheduleGenerationSettings();

                _context.ScheduleGenerationSettings.Add(settings);
            }

            settings.DefaultWeeklyLessons =
                model.DefaultWeeklyLessons;

            settings.MaxLessonsPerDay =
                model.MaxLessonsPerDay;

            settings.TeachingDaysPerWeek =
                model.TeachingDaysPerWeek;

            settings.AllowGaps =
                model.AllowGaps;

            settings.MaxGapsPerDay =
                model.MaxGapsPerDay;

            await _context.SaveChangesAsync();

            TempData["ProgramMessage"] =
                "Настройки автоматического составления расписания сохранены.";

            return RedirectToAction(nameof(Settings));
        }


        // =========================================================
        // ADD / UPDATE GROUP SUBJECT
        // =========================================================

        [HttpPost("/Account/AddGroupSubject")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGroupSubject(
            int groupId,
            int subjectId,
            int weeklyLessons)
        {
            // -----------------------------------------------------
            // Проверяем количество занятий
            // -----------------------------------------------------

            if (weeklyLessons < 1 ||
                weeklyLessons > 20)
            {
                TempData["ProgramMessage"] =
                    "Количество пар в неделю должно быть от 1 до 20.";

                return RedirectToAction(nameof(Settings));
            }

            // -----------------------------------------------------
            // Проверяем группу
            // -----------------------------------------------------

            var groupExists =
                await _context.Groups
                    .AnyAsync(g => g.Id == groupId);

            if (!groupExists)
            {
                TempData["ProgramMessage"] =
                    "Выбранная группа не найдена.";

                return RedirectToAction(nameof(Settings));
            }

            // -----------------------------------------------------
            // Проверяем предмет
            // -----------------------------------------------------

            var subjectExists =
                await _context.Subjects
                    .AnyAsync(s => s.Id == subjectId);

            if (!subjectExists)
            {
                TempData["ProgramMessage"] =
                    "Выбранный предмет не найден.";

                return RedirectToAction(nameof(Settings));
            }

            // -----------------------------------------------------
            // Проверяем существующую связь
            // -----------------------------------------------------

            var existing =
                await _context.GroupSubjects
                    .FirstOrDefaultAsync(gs =>
                        gs.GroupId == groupId &&
                        gs.SubjectId == subjectId);

            if (existing != null)
            {
                existing.WeeklyLessons =
                    weeklyLessons;

                TempData["ProgramMessage"] =
                    "Учебная нагрузка обновлена.";
            }
            else
            {
                var groupSubject =
                    new GroupSubject
                    {
                        GroupId = groupId,
                        SubjectId = subjectId,
                        WeeklyLessons = weeklyLessons
                    };

                _context.GroupSubjects.Add(
                    groupSubject);

                TempData["ProgramMessage"] =
                    "Учебная нагрузка добавлена.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Settings));
        }


        // =========================================================
        // SAVE MULTIPLE GROUP SUBJECTS
        // =========================================================
        //
        // Позволяет:
        // 1. Добавлять несколько предметов.
        // 2. Изменять уже существующую учебную нагрузку.
        // 3. Удалять существующие предметы.
        // 4. Сохранять все изменения одним SaveChangesAsync().
        //
        // =========================================================

        [HttpPost("/Account/SaveGroupSubjects")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroupSubjects()
        {
            // =========================================================
            // ПОЛУЧАЕМ ДАННЫЕ ИЗ ФОРМЫ
            // =========================================================

            var groupIds =
                Request.Form["groupId"]
                    .Select(value =>
                    {
                        int.TryParse(value, out var result);
                        return result;
                    })
                    .ToList();

            var subjectIds =
                Request.Form["subjectId"]
                    .Select(value =>
                    {
                        int.TryParse(value, out var result);
                        return result;
                    })
                    .ToList();

            var weeklyLessons =
                Request.Form["weeklyLessons"]
                    .Select(value =>
                    {
                        int.TryParse(value, out var result);
                        return result;
                    })
                    .ToList();

            // =========================================================
            // ПОЛУЧАЕМ ID ЗАПИСЕЙ, КОТОРЫЕ НУЖНО УДАЛИТЬ
            // =========================================================

            var deletedIds =
                Request.Form["deletedGroupSubjectId"]
                    .Select(value =>
                    {
                        int.TryParse(value, out var result);
                        return result;
                    })
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

            // =========================================================
            // ПОДГОТАВЛИВАЕМ СПИСОК ДОБАВЛЕНИЙ / ИЗМЕНЕНИЙ
            // =========================================================

            var items =
                new List<(int GroupId, int SubjectId, int WeeklyLessons)>();

            // Если строки присутствуют, проверяем их.
            if (groupIds.Count > 0 ||
                subjectIds.Count > 0 ||
                weeklyLessons.Count > 0)
            {
                // -----------------------------------------------------
                // Все три массива должны иметь одинаковую длину
                // -----------------------------------------------------

                if (groupIds.Count != subjectIds.Count ||
                    groupIds.Count != weeklyLessons.Count)
                {
                    TempData["ProgramMessage"] =
                        "Не удалось обработать учебную нагрузку. " +
                        "Количество данных в строках не совпадает.";

                    return RedirectToAction(nameof(Settings));
                }

                // -----------------------------------------------------
                // Формируем записи
                // -----------------------------------------------------

                for (var i = 0; i < groupIds.Count; i++)
                {
                    var groupId = groupIds[i];
                    var subjectId = subjectIds[i];
                    var lessons = weeklyLessons[i];

                    // Пустые строки пропускаем
                    if (groupId <= 0 ||
                        subjectId <= 0)
                    {
                        continue;
                    }

                    // Проверяем количество занятий
                    if (lessons < 1 ||
                        lessons > 20)
                    {
                        TempData["ProgramMessage"] =
                            "Количество пар в неделю должно быть от 1 до 20.";

                        return RedirectToAction(nameof(Settings));
                    }

                    items.Add(
                        (
                            groupId,
                            subjectId,
                            lessons
                        ));
                }
            }

            // =========================================================
            // УБИРАЕМ ДУБЛИКАТЫ
            // =========================================================
            //
            // Если одна группа + один предмет встречаются несколько
            // раз, используется последнее значение.
            //
            // =========================================================

            var uniqueItems =
                items
                    .GroupBy(x => new
                    {
                        x.GroupId,
                        x.SubjectId
                    })
                    .Select(group =>
                        group.Last())
                    .ToList();

            // =========================================================
            // ПРОВЕРЯЕМ ГРУППЫ
            // =========================================================

            var groupIdsToCheck =
                uniqueItems
                    .Select(x => x.GroupId)
                    .Distinct()
                    .ToList();

            if (groupIdsToCheck.Count > 0)
            {
                var existingGroupIds =
                    await _context.Groups
                        .Where(g =>
                            groupIdsToCheck.Contains(g.Id))
                        .Select(g => g.Id)
                        .ToListAsync();

                if (existingGroupIds.Count !=
                    groupIdsToCheck.Count)
                {
                    TempData["ProgramMessage"] =
                        "Одна или несколько выбранных групп не найдены.";

                    return RedirectToAction(nameof(Settings));
                }
            }

            // =========================================================
            // ПРОВЕРЯЕМ ПРЕДМЕТЫ
            // =========================================================

            var subjectIdsToCheck =
                uniqueItems
                    .Select(x => x.SubjectId)
                    .Distinct()
                    .ToList();

            if (subjectIdsToCheck.Count > 0)
            {
                var existingSubjectIds =
                    await _context.Subjects
                        .Where(s =>
                            subjectIdsToCheck.Contains(s.Id))
                        .Select(s => s.Id)
                        .ToListAsync();

                if (existingSubjectIds.Count !=
                    subjectIdsToCheck.Count)
                {
                    TempData["ProgramMessage"] =
                        "Один или несколько выбранных предметов не найдены.";

                    return RedirectToAction(nameof(Settings));
                }
            }

            // =========================================================
            // УДАЛЕНИЕ
            // =========================================================

            var deletedCount = 0;

            if (deletedIds.Count > 0)
            {
                var itemsToDelete =
                    await _context.GroupSubjects
                        .Where(gs =>
                            deletedIds.Contains(gs.Id))
                        .ToListAsync();

                if (itemsToDelete.Count > 0)
                {
                    deletedCount =
                        itemsToDelete.Count;

                    _context.GroupSubjects.RemoveRange(
                        itemsToDelete);
                }
            }

            // =========================================================
            // ЗАГРУЖАЕМ СУЩЕСТВУЮЩИЕ ЗАПИСИ
            // =========================================================

            var existingGroupSubjects =
                new List<GroupSubject>();

            if (groupIdsToCheck.Count > 0 &&
                subjectIdsToCheck.Count > 0)
            {
                existingGroupSubjects =
                    await _context.GroupSubjects
                        .Where(gs =>
                            groupIdsToCheck.Contains(gs.GroupId) &&
                            subjectIdsToCheck.Contains(gs.SubjectId))
                        .ToListAsync();
            }

            // =========================================================
            // ДОБАВЛЕНИЕ / ИЗМЕНЕНИЕ
            // =========================================================

            var addedCount = 0;
            var updatedCount = 0;

            foreach (var item in uniqueItems)
            {
                var existing =
                    existingGroupSubjects
                        .FirstOrDefault(gs =>
                            gs.GroupId == item.GroupId &&
                            gs.SubjectId == item.SubjectId);

                if (existing != null)
                {
                    // -------------------------------------------------
                    // Существующая запись → изменяем
                    // -------------------------------------------------

                    if (existing.WeeklyLessons !=
                        item.WeeklyLessons)
                    {
                        existing.WeeklyLessons =
                            item.WeeklyLessons;

                        updatedCount++;
                    }
                }
                else
                {
                    // -------------------------------------------------
                    // Новая запись → создаём
                    // -------------------------------------------------

                    var groupSubject =
                        new GroupSubject
                        {
                            GroupId = item.GroupId,
                            SubjectId = item.SubjectId,
                            WeeklyLessons =
                                item.WeeklyLessons
                        };

                    _context.GroupSubjects.Add(
                        groupSubject);

                    addedCount++;
                }
            }

            // =========================================================
            // ЕСЛИ ВООБЩЕ НЕТ ИЗМЕНЕНИЙ
            // =========================================================

            if (addedCount == 0 &&
                updatedCount == 0 &&
                deletedCount == 0)
            {
                TempData["ProgramMessage"] =
                    "Изменений в учебной нагрузке нет.";

                return RedirectToAction(nameof(Settings));
            }

            // =========================================================
            // СОХРАНЯЕМ ВСЁ ОДНИМ SaveChangesAsync()
            // =========================================================

            await _context.SaveChangesAsync();

            // =========================================================
            // ФОРМИРУЕМ СООБЩЕНИЕ
            // =========================================================

            var messageParts =
                new List<string>();

            if (addedCount > 0)
            {
                messageParts.Add(
                    $"добавлено: {addedCount}");
            }

            if (updatedCount > 0)
            {
                messageParts.Add(
                    $"изменено: {updatedCount}");
            }

            if (deletedCount > 0)
            {
                messageParts.Add(
                    $"удалено: {deletedCount}");
            }

            TempData["ProgramMessage"] =
                "Учебная нагрузка сохранена. " +
                string.Join(", ", messageParts) +
                ".";

            return RedirectToAction(nameof(Settings));
        }
        // =========================================================
        // UPDATE PROFILE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            string email,
            string displayName)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            email =
                email?.Trim() ?? "";

            displayName =
                displayName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "email",
                    "Введите Email.");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                ModelState.AddModelError(
                    "displayName",
                    "Введите имя пользователя.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSettingsUserDataAsync(user);

                return View("Settings");
            }

            // -----------------------------------------------------
            // Проверяем изменение email
            // -----------------------------------------------------

            if (!string.Equals(
                    email,
                    user.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var existingUser =
                    await _userManager.FindByEmailAsync(email);

                if (existingUser != null &&
                    existingUser.Id != user.Id)
                {
                    ModelState.AddModelError(
                        "email",
                        "Пользователь с таким Email уже существует.");

                    await LoadSettingsUserDataAsync(user);

                    return View("Settings");
                }

                var emailResult =
                    await _userManager.SetEmailAsync(
                        user,
                        email);

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError(
                            "email",
                            error.Description);
                    }

                    await LoadSettingsUserDataAsync(user);

                    return View("Settings");
                }

                var userNameResult =
                    await _userManager.SetUserNameAsync(
                        user,
                        email);

                if (!userNameResult.Succeeded)
                {
                    foreach (var error in userNameResult.Errors)
                    {
                        ModelState.AddModelError(
                            "email",
                            error.Description);
                    }

                    await LoadSettingsUserDataAsync(user);

                    return View("Settings");
                }
            }

            // -----------------------------------------------------
            // DisplayName
            // -----------------------------------------------------

            user.DisplayName =
                displayName;

            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                await LoadSettingsUserDataAsync(user);

                return View("Settings");
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] =
                "Данные профиля успешно обновлены.";

            return RedirectToAction(nameof(Settings));
        }

        // =========================================================
        // LOAD SETTINGS USER DATA
        // =========================================================

        private async Task LoadSettingsUserDataAsync(
            ApplicationUser user)
        {
            ViewBag.Email =
                user.Email ?? "";

            ViewBag.UserName =
                user.UserName ??
                user.Email ??
                "";

            ViewBag.DisplayName =
                user.DisplayName ?? "";

            ViewBag.IsAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin");

            if (User.IsInRole("Admin"))
            {
                await LoadProgramSettingsDataAsync();
            }
        }

        // =========================================================
        // ACCESS DENIED
        // =========================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }


}
