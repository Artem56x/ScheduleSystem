using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleSystem.Data;
using ScheduleSystem.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ScheduleExportService>();
builder.Services.AddScoped<ScheduleValidationService>();
builder.Services.AddScoped<ClassroomRecommendationService>();


builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        fieldName => $"Пожалуйста, заполните поле «{fieldName}».");
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();