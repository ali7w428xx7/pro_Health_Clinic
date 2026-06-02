using ClinicReport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicReport.Controllers;

public class ReportsController : Controller
{
    private readonly IReportApiClient _api;

    public ReportsController(IReportApiClient api) => _api = api;

    private IActionResult RequireAuth()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt")))
            return RedirectToAction("Login", "Account");
        return null!;
    }

    public async Task<IActionResult> Appointments(string? from, string? to)
    {
        var redirect = RequireAuth();
        if (redirect != null) return redirect;

        ViewBag.From = from;
        ViewBag.To = to;

        var stats = await _api.GetAppointmentStatsAsync(from, to);
        return View(stats);
    }

    public async Task<IActionResult> DoctorUtilization(string? from, string? to)
    {
        var redirect = RequireAuth();
        if (redirect != null) return redirect;

        ViewBag.From = from;
        ViewBag.To = to;

        var data = await _api.GetDoctorUtilizationAsync(from, to);
        return View(data);
    }

    public async Task<IActionResult> CancellationRates(string? from, string? to)
    {
        var redirect = RequireAuth();
        if (redirect != null) return redirect;

        ViewBag.From = from;
        ViewBag.To = to;

        var data = await _api.GetCancellationRatesAsync(from, to);
        return View(data);
    }
}
