using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Services;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Identity;
using ScheduleSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// IDENTITY
// ============================================================

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ============================================================
// CACHE / SESSION
// ============================================================

builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.Cookie.Name = "ScheduleSystem.Session";
});

// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<ScheduleExportService>();
builder.Services.AddScoped<ScheduleValidationService>();
builder.Services.AddScoped<ClassroomRecommendationService>();
builder.Services.AddScoped<ScheduleGeneratorService>();
builder.Services.AddScoped<TeacherRecommendationService>();

// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        fieldName =>
            $"Пожалуйста, заполните поле «{fieldName}».");
});

// ============================================================
// QUESTPDF
// ============================================================

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// ============================================================
// IDENTITY SEED
// ============================================================

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

// ============================================================
// MIDDLEWARE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// Session должна быть после Routing
// и до контроллеров.
app.UseSession();

app.UseAuthorization();

// ============================================================
// ROUTING
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();