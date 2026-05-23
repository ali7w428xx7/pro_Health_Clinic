using System.Diagnostics;
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers;

public class HomeController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ClinicDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var today = DateOnly.FromDateTime(DateTime.Today);

        ViewBag.Role = user.Role.ToString();
        ViewBag.FullName = user.FullName;

        if (user.Role == UserRole.ClinicManager)
        {
            ViewBag.TotalAppointmentsToday = await _db.Appointments.CountAsync(a => a.AppointmentDate == today);
            ViewBag.PendingConfirmations = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Requested);
            ViewBag.ActiveDoctors = await _db.Doctors.CountAsync(d => d.IsActive);
            ViewBag.TotalPatients = await _db.Patients.CountAsync();
        }
        else if (user.Role == UserRole.Doctor)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);
            if (doctor != null)
            {
                ViewBag.TodayAppointments = await _db.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate == today && a.Status != AppointmentStatus.Cancelled);
                ViewBag.PendingAppointments = await _db.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.CheckedIn);
            }
        }
        else if (user.Role == UserRole.Patient)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);
            if (patient != null)
            {
                ViewBag.UpcomingAppointments = await _db.Appointments
                    .CountAsync(a => a.PatientId == patient.Id && a.AppointmentDate >= today && a.Status != AppointmentStatus.Cancelled);
            }
        }
        else if (user.Role == UserRole.Receptionist)
        {
            ViewBag.TodayAppointments = await _db.Appointments.CountAsync(a => a.AppointmentDate == today);
            ViewBag.AwaitingCheckIn = await _db.Appointments.CountAsync(a => a.AppointmentDate == today && a.Status == AppointmentStatus.Confirmed);
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
