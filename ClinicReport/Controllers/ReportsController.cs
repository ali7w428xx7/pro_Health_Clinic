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

    public async Task<IActionResult> Appointments()
    {
        var redirect = RequireAuth();
        if (redirect != null) return redirect;

        var stats = await _api.GetAppointmentStatsAsync();
        return View(stats);
    }

    public async Task<IActionResult> DoctorUtilization()
    {
        var redirect = RequireAuth();
        if (redirect != null) return redirect;

        var data = await _api.GetDoctorUtilizationAsync();
        return View(data);
    }

    public async Task<IActionResult> CancellationRates()
    {
        var redirect = RequireAuth();
        if (redirect != null) return redirect;

        var data = await _api.GetCancellationRatesAsync();
        return View(data);
    }
}
