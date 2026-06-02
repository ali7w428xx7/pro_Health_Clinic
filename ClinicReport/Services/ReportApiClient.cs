using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicReport.Models;

namespace ClinicReport.Services;

public interface IReportApiClient
{
    Task<AuthResponse?> LoginAsync(string email, string password);
    Task<AppointmentStatsDto?> GetAppointmentStatsAsync(string? from = null, string? to = null);
    Task<List<DoctorUtilizationDto>> GetDoctorUtilizationAsync(string? from = null, string? to = null);
    Task<CancellationRateDto?> GetCancellationRatesAsync(string? from = null, string? to = null);
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

    private static string BuildDateQuery(string? from, string? to)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(from)) parts.Add($"from={Uri.EscapeDataString(from)}");
        if (!string.IsNullOrWhiteSpace(to)) parts.Add($"to={Uri.EscapeDataString(to)}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    public async Task<AppointmentStatsDto?> GetAppointmentStatsAsync(string? from = null, string? to = null)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/reports/appointment-statistics{BuildDateQuery(from, to)}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AppointmentStatsDto>(json, JsonOpts);
    }

    public async Task<List<DoctorUtilizationDto>> GetDoctorUtilizationAsync(string? from = null, string? to = null)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/reports/doctor-utilization{BuildDateQuery(from, to)}");
        if (!response.IsSuccessStatusCode) return [];
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DoctorUtilizationDto>>(json, JsonOpts) ?? [];
    }

    public async Task<CancellationRateDto?> GetCancellationRatesAsync(string? from = null, string? to = null)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/reports/cancellation-rates{BuildDateQuery(from, to)}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CancellationRateDto>(json, JsonOpts);
    }
}
