using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Mitc_report_Update.BackgroundWorker;
using Mitc_report_Update.Configuration;
using Mitc_report_Update.Implementation.MailingService;
using Mitc_report_Update.Implementation.TemplateEngine;
using Mitc_report_Update.Interface.MailingService;
using Mitc_report_Update.Interface.TemplateEngine;
using MITCRMS.Identity;
using MITCRMS.Implementation.Messaging;
using MITCRMS.Implementation.Repository;
using MITCRMS.Implementation.Services;
using MITCRMS.Interface.Messaging;
using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs.Report.Validation;
using MITCRMS.Models.Entities;
using MITCRMS.Persistence.Context;
using Scrutor;
using System;


var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<MitcrmsContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MitcrmsContext"),
        new MySqlServerVersion(new Version(9, 0, 0))
    ));

// MVC + Razor Pages
builder.Services.AddControllersWithViews();

// Scrutor: automatic registration by convention
builder.Services.Scan(scan => scan
    // scan assemblies that start with your project's root name
    .FromApplicationDependencies(a => a.FullName != null && a.FullName.StartsWith("MITCRMS"))
    // register all *Service classes as their implemented interfaces
    .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Services")))
        .AsImplementedInterfaces()
        .WithScopedLifetime()
    // register all *Repository classes as their implemented interfaces
    .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Repository") || t.Name.EndsWith("Repositories")))
        .AsImplementedInterfaces()
         .WithScopedLifetime()
);


builder.Services.AddHostedService<WeeklyReportReminderBackgroundWorker>();
builder.Services.AddScoped<IMailService, MailService>();
// explicit Identity stores and identity configuration (keep explicit � avoids accidental overrides)
builder.Services.AddScoped<IUserStore<User>, UserStore>();
builder.Services.AddScoped<IRoleStore<Role>, RoleStore>();
builder.Services.AddIdentity<User, Role>()
    .AddDefaultTokenProviders();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddScoped<IMailSender, MailSender>();
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddScoped<IRazorEngine, RazorEngine>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateReportValidation>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();



builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/Login";
    options.LogoutPath = "/User/Logout";
    options.Cookie.Name = "MITCRMS";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Build
var app = builder.Build();

// Error handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

try
{
    app.MapStaticAssets();
}
catch
{
    // ignore if extension not present
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}")
    .WithStaticAssets();



app.Run();
