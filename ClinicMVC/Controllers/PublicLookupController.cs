using ClinicMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMVC.Controllers;

// This controller is the ONLY feature that calls the API via HttpClient
public class PublicLookupController : Controller
{
    private readonly IClinicApiClient _apiClient;

    public PublicLookupController(IClinicApiClient apiClient) => _apiClient = apiClient;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string cprNumber, string patientReferenceNumber)
    {
        if (string.IsNullOrWhiteSpace(cprNumber) || string.IsNullOrWhiteSpace(patientReferenceNumber))
        {
            ModelState.AddModelError("", "Both CPR number and reference number are required.");
            return View();
        }

        var result = await _apiClient.LookupAppointmentsAsync(cprNumber.Trim(), patientReferenceNumber.Trim());

        if (result == null)
        {
            ViewBag.NotFound = true;
            return View();
        }

        return View("Result", result);
    }
}
