using eDrushti_Exam.App.Models;
using eDrushti_Exam.App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eDrushti_Exam.App.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var pwd = BCrypt.Net.BCrypt.HashPassword("Pass@1234");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var candidate = await _authService.ValidateCandidateAsync(model.Email, model.Password);

            if (candidate == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, candidate.Id.ToString()),
                new Claim(ClaimTypes.Name,           candidate.FullName),
                new Claim(ClaimTypes.Email,          candidate.Email),
                new Claim(ClaimTypes.Role,           candidate.IsAdmin ? "Admin" : "Candidate"),
                new Claim("TrackId",                 candidate.TrackId.ToString()),
                new Claim("TrackName",               candidate.Track?.Name ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false }
            );

            if (candidate.IsAdmin)
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Interview");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
