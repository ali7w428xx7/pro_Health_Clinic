using System.Diagnostics;
using ClinicReport.Models;
using ClinicReport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicReport.Controllers;

public class HomeController : Controller
{
    private readonly IReportApiClient _api;

    public HomeController(IReportApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt")))
            return RedirectToAction("Login", "Account");

        var stats = await _api.GetAppointmentStatsAsync();
        var utilization = await _api.GetDoctorUtilizationAsync();
        var cancellations = await _api.GetCancellationRatesAsync();

        ViewBag.Stats = stats;
        ViewBag.Utilization = utilization;
        ViewBag.Cancellations = cancellations;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
