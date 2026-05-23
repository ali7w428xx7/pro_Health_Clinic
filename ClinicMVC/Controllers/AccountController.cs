using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ClinicDbContext _db;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, ClinicDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        if (result.IsLockedOut)
            ModelState.AddModelError("", "Account locked out. Please try again later.");
        else
            ModelState.AddModelError("", "Invalid email or password.");

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterPatientViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _db.Patients.AnyAsync(p => p.CPRNumber == model.CPRNumber))
        {
            ModelState.AddModelError(nameof(model.CPRNumber), "This CPR number is already registered.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Role = UserRole.Patient,
            CPRNumber = model.CPRNumber,
            PatientReferenceNumber = $"PAT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Patient");
        _db.Patients.Add(new Patient
        {
            ApplicationUserId = user.Id,
            CPRNumber = model.CPRNumber,
            PatientReferenceNumber = user.PatientReferenceNumber!,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Phone = model.Phone,
            Address = model.Address,
            BloodType = model.BloodType,
            Allergies = model.Allergies
        });
        await _db.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = "Registration successful. Welcome!";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "ClinicManager")]
    public async Task<IActionResult> CreateStaff()
    {
        var vm = new CreateStaffViewModel();
        ViewBag.Specializations = await _db.Specializations
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToListAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "ClinicManager")]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        ViewBag.Specializations = await _db.Specializations
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToListAsync();

        if (model.Role == UserRole.Doctor && string.IsNullOrWhiteSpace(model.LicenseNumber))
            ModelState.AddModelError(nameof(model.LicenseNumber), "License number is required for doctors.");

        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Role = model.Role,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role.ToString());

        if (model.Role == UserRole.Doctor)
        {
            var doctor = new Doctor
            {
                ApplicationUserId = user.Id,
                LicenseNumber = model.LicenseNumber!,
                Bio = model.Bio ?? "",
                IsActive = true
            };
            _db.Doctors.Add(doctor);
            await _db.SaveChangesAsync();

            foreach (var specId in model.SpecializationIds)
                _db.DoctorSpecializations.Add(new DoctorSpecialization { DoctorId = doctor.Id, SpecializationId = specId });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"{model.Role} account created successfully.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
