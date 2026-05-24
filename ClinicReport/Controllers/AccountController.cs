using ClinicReport.Models;
using ClinicReport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicReport.Controllers;

public class AccountController : Controller
{
    private readonly IReportApiClient _api;

    public AccountController(IReportApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var auth = await _api.LoginAsync(model.Email, model.Password);
        if (auth == null || auth.Role != "ClinicManager")
        {
            ModelState.AddModelError("", auth == null
                ? "Invalid email or password."
                : "Access restricted to Clinic Managers.");
            return View(model);
        }

        HttpContext.Session.SetString("jwt", auth.Token);
        HttpContext.Session.SetString("fullName", auth.FullName);
        HttpContext.Session.SetString("email", auth.Email);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
