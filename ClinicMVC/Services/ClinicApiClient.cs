using ClinicAPI.DTOs;
using System.Text.Json;

namespace ClinicMVC.Services;

public interface IClinicApiClient
{
    Task<PublicLookupResultDto?> LookupAppointmentsAsync(string cprNumber, string referenceNumber);
}

public class ClinicApiClient : IClinicApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ClinicApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PublicLookupResultDto?> LookupAppointmentsAsync(string cprNumber, string referenceNumber)
    {
        var client = _httpClientFactory.CreateClient("ClinicAPI");
        var response = await client.GetAsync(
            $"api/appointments/lookup?cprNumber={Uri.EscapeDataString(cprNumber)}&patientReferenceNumber={Uri.EscapeDataString(referenceNumber)}");

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PublicLookupResultDto>(json, _jsonOptions);
    }
}
