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
        // PROGRAM SETTINGS
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ProgramSettings()
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

            return View();
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
                    nameof(ProgramSettings));
            }

            _context.Schedules.RemoveRange(
                schedules);

            await _context.SaveChangesAsync();

            TempData["ProgramMessage"] =
                $"Расписание успешно очищено. " +
                $"Удалено занятий: {schedules.Count}.";

            return RedirectToAction(
                nameof(ProgramSettings));
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
                    nameof(ProgramSettings));
            }


            /*
             * Сначала удаляем расписание,
             * поскольку оно содержит ссылки
             * на преподавателей, группы,
             * предметы и аудитории.
             */

            _context.Schedules.RemoveRange(
                schedules);


            /*
             * Затем удаляем преподавателей
             * и группы.
             */

            _context.Teachers.RemoveRange(
                teachers);

            _context.Groups.RemoveRange(
                groups);


            /*
             * Затем удаляем аудитории.
             */

            _context.Classrooms.RemoveRange(
                classrooms);


            /*
             * Затем удаляем предметы.
             */

            _context.Subjects.RemoveRange(
                subjects);


            /*
             * В самом конце удаляем
             * категории аудиторий.
             */

            _context.ClassroomCategories.RemoveRange(
                classroomCategories);


            await _context.SaveChangesAsync();


            TempData["ProgramMessage"] =
                $"Все данные системы успешно сброшены. " +
                $"Удалено записей: {totalDeleted}.";


            return RedirectToAction(
                nameof(ProgramSettings));
        }


        // =========================================================
        // LOGIN
        // =========================================================

        [HttpGet]
        public IActionResult Login(
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] =
                returnUrl;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] =
                returnUrl;


            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Введите email и пароль.");

                return View();
            }


            var user =
                await _userManager.FindByEmailAsync(
                    email);


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
                return RedirectToAction(
                    nameof(Login));
            }


            var user =
                await _userManager.GetUserAsync(
                    User);


            if (user == null)
            {
                return RedirectToAction(
                    nameof(Login));
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
                await _userManager.GetUserAsync(
                    User);


            if (user == null)
            {
                return RedirectToAction(
                    nameof(Login));
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


            await _signInManager.RefreshSignInAsync(
                user);


            TempData["SuccessMessage"] =
                "Пароль успешно изменён.";


            return RedirectToAction(
                nameof(Settings));
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
                await _userManager.FindByEmailAsync(
                    email);


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
                return RedirectToAction(
                    nameof(Login));
            }


            var user =
                await _userManager.GetUserAsync(
                    User);


            if (user == null)
            {
                return RedirectToAction(
                    nameof(Login));
            }


            ViewBag.Email =
                user.Email;

            ViewBag.DisplayName =
                user.DisplayName;

            ViewBag.IsAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin");


            return View();
        }


        // =========================================================
        // SETTINGS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(
                    nameof(Login));
            }


            var user =
                await _userManager.GetUserAsync(
                    User);


            if (user == null)
            {
                return RedirectToAction(
                    nameof(Login));
            }


            ViewBag.Email =
                user.Email;

            ViewBag.UserName =
                user.UserName;

            ViewBag.DisplayName =
                user.DisplayName;

            ViewBag.IsAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin");


            return View();
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
                return RedirectToAction(
                    nameof(Login));
            }


            var user =
                await _userManager.GetUserAsync(
                    User);


            if (user == null)
            {
                return RedirectToAction(
                    nameof(Login));
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
                ViewBag.Email =
                    user.Email;

                ViewBag.UserName =
                    user.UserName;

                ViewBag.DisplayName =
                    user.DisplayName;

                ViewBag.IsAdmin =
                    await _userManager.IsInRoleAsync(
                        user,
                        "Admin");

                return View("Settings");
            }


            if (!string.Equals(
                    email,
                    user.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var existingUser =
                    await _userManager.FindByEmailAsync(
                        email);


                if (existingUser != null &&
                    existingUser.Id != user.Id)
                {
                    ModelState.AddModelError(
                        "email",
                        "Пользователь с таким Email уже существует.");


                    ViewBag.Email =
                        user.Email;

                    ViewBag.UserName =
                        user.UserName;

                    ViewBag.DisplayName =
                        user.DisplayName;

                    ViewBag.IsAdmin =
                        await _userManager.IsInRoleAsync(
                            user,
                            "Admin");

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


                    ViewBag.Email =
                        user.Email;

                    ViewBag.UserName =
                        user.UserName;

                    ViewBag.DisplayName =
                        user.DisplayName;

                    ViewBag.IsAdmin =
                        await _userManager.IsInRoleAsync(
                            user,
                            "Admin");

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


                    ViewBag.Email =
                        user.Email;

                    ViewBag.UserName =
                        user.UserName;

                    ViewBag.DisplayName =
                        user.DisplayName;

                    ViewBag.IsAdmin =
                        await _userManager.IsInRoleAsync(
                            user,
                            "Admin");

                    return View("Settings");
                }
            }


            user.DisplayName =
                displayName;


            var updateResult =
                await _userManager.UpdateAsync(
                    user);


            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }


                ViewBag.Email =
                    user.Email;

                ViewBag.UserName =
                    user.UserName;

                ViewBag.DisplayName =
                    user.DisplayName;

                ViewBag.IsAdmin =
                    await _userManager.IsInRoleAsync(
                        user,
                        "Admin");

                return View("Settings");
            }


            await _signInManager.RefreshSignInAsync(
                user);


            TempData["SuccessMessage"] =
                "Данные профиля успешно обновлены.";


            return RedirectToAction(
                nameof(Settings));
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
