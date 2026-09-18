using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScheduleSystem.Models;

namespace ScheduleSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

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

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Неверный email или пароль.");

                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
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

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                "",
                "Неверный email или пароль.");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.GetUserAsync(User);

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
                ModelState.AddModelError("", "Заполните все поля.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Новые пароли не совпадают.");
                return View();
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Новый пароль должен содержать минимум 6 символов.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                currentPassword,
                newPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Пароль успешно изменён.";

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

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

            var existingUser = await _userManager.FindByEmailAsync(email);

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

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }

            await _userManager.AddToRoleAsync(user, "User");

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.Email = user.Email;
            ViewBag.DisplayName = user.DisplayName;
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.Email = user.Email;
            ViewBag.UserName = user.UserName;
            ViewBag.DisplayName = user.DisplayName;
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            return View();
        }

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

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            email = email?.Trim() ?? "";
            displayName = displayName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("email", "Введите Email.");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                ModelState.AddModelError("displayName", "Введите имя пользователя.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Email = user.Email;
                ViewBag.UserName = user.UserName;
                ViewBag.DisplayName = user.DisplayName;
                ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                return View("Settings");
            }

            if (!string.Equals(
                    email,
                    user.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByEmailAsync(email);

                if (existingUser != null &&
                    existingUser.Id != user.Id)
                {
                    ModelState.AddModelError(
                        "email",
                        "Пользователь с таким Email уже существует.");

                    ViewBag.Email = user.Email;
                    ViewBag.UserName = user.UserName;
                    ViewBag.DisplayName = user.DisplayName;
                    ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                    return View("Settings");
                }

                var emailResult = await _userManager.SetEmailAsync(user, email);

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError("email", error.Description);
                    }

                    ViewBag.Email = user.Email;
                    ViewBag.UserName = user.UserName;
                    ViewBag.DisplayName = user.DisplayName;
                    ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                    return View("Settings");
                }

                var userNameResult = await _userManager.SetUserNameAsync(user, email);

                if (!userNameResult.Succeeded)
                {
                    foreach (var error in userNameResult.Errors)
                    {
                        ModelState.AddModelError("email", error.Description);
                    }

                    ViewBag.Email = user.Email;
                    ViewBag.UserName = user.UserName;
                    ViewBag.DisplayName = user.DisplayName;
                    ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                    return View("Settings");
                }
            }

            user.DisplayName = displayName;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ViewBag.Email = user.Email;
                ViewBag.UserName = user.UserName;
                ViewBag.DisplayName = user.DisplayName;
                ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                return View("Settings");
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] =
                "Данные профиля успешно обновлены.";

            return RedirectToAction(nameof(Settings));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}