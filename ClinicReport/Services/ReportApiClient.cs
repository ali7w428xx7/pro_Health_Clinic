using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicReport.Models;

namespace ClinicReport.Services;

public interface IReportApiClient
{
    Task<AuthResponse?> LoginAsync(string email, string password);
    Task<AppointmentStatsDto?> GetAppointmentStatsAsync();
    Task<List<DoctorUtilizationDto>> GetDoctorUtilizationAsync();
    Task<CancellationRateDto?> GetCancellationRatesAsync();
}

public class ReportApiClient : IReportApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ReportApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
    {
        _http = factory.CreateClient("ClinicAPI");
        _accessor = accessor;
    }

    private void AttachToken()
    {
        var token = _accessor.HttpContext?.Session.GetString("jwt");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var body = JsonSerializer.Serialize(new { email, password });
        var response = await _http.PostAsync("/api/auth/login",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponse>(json, JsonOpts);
    }

    public async Task<AppointmentStatsDto?> GetAppointmentStatsAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("/api/reports/appointment-statistics");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AppointmentStatsDto>(json, JsonOpts);
    }

    public async Task<List<DoctorUtilizationDto>> GetDoctorUtilizationAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("/api/reports/doctor-utilization");
        if (!response.IsSuccessStatusCode) return [];
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DoctorUtilizationDto>>(json, JsonOpts) ?? [];
    }

    public async Task<CancellationRateDto?> GetCancellationRatesAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("/api/reports/cancellation-rates");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CancellationRateDto>(json, JsonOpts);
    }
}
