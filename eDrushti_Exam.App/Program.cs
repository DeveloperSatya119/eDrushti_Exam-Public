// ── Program.cs  (relevant additions only) ────────────────────────────────────
using eDrushti_Exam.App.Data;
using eDrushti_Exam.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Auth/Login";
        opt.LogoutPath = "/Auth/Logout";
        opt.AccessDeniedPath = "/Auth/Login";
        opt.ExpireTimeSpan = TimeSpan.FromHours(2);
        opt.SlidingExpiration = true;
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();      
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Auth}/{action=Login}/{id?}");

await app.RunAsync();