namespace ClinicReport.Models;

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime Expiry { get; set; }
}

public class AppointmentStatsDto
{
    public int TotalAppointments { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Pending { get; set; }
    public double CompletionRate { get; set; }
    public List<MonthlyStatDto> MonthlyBreakdown { get; set; } = [];
    public List<SpecializationStatDto> BySpecialization { get; set; } = [];
}

public class MonthlyStatDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
}

public class SpecializationStatDto
{
    public string Specialization { get; set; } = "";
    public int Total { get; set; }
    public int Completed { get; set; }
}

public class DoctorUtilizationDto
{
    public string DoctorName { get; set; } = "";
    public int TotalSlots { get; set; }
    public int BookedSlots { get; set; }
    public double UtilizationRate { get; set; }
    public int CompletedAppointments { get; set; }
    public List<string> Specializations { get; set; } = [];
}

public class CancellationRateDto
{
    public double OverallCancellationRate { get; set; }
    public int TotalCancelled { get; set; }
    public int TotalAppointments { get; set; }
    public List<DoctorCancellationDto> ByDoctor { get; set; } = [];
}

public class DoctorCancellationDto
{
    public string DoctorName { get; set; } = "";
    public int Cancelled { get; set; }
    public int Total { get; set; }
    public double Rate { get; set; }
}
