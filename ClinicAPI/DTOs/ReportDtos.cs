namespace ClinicAPI.DTOs;

public class AppointmentStatsDto
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Missed { get; set; }
    public int Requested { get; set; }
    public int Confirmed { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public double NoShowRate { get; set; }
    public List<MonthlyStatDto> ByMonth { get; set; } = [];
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
    public string Name { get; set; } = "";
    public int Total { get; set; }
}

public class DoctorUtilizationDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public List<string> Specializations { get; set; } = [];
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public double UtilizationRate { get; set; }
}

public class CancellationRateDto
{
    public double OverallCancellationRate { get; set; }
    public double OverallNoShowRate { get; set; }
    public List<DoctorCancellationDto> ByDoctor { get; set; } = [];
}

public class DoctorCancellationDto
{
    public string DoctorName { get; set; } = "";
    public int Total { get; set; }
    public int Cancelled { get; set; }
    public int Missed { get; set; }
    public double CancellationRate { get; set; }
    public double NoShowRate { get; set; }
}
