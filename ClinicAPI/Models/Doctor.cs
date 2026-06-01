namespace ClinicAPI.Models;

public class Doctor
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = "";
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public string LicenseNumber { get; set; } = "";
    public string Bio { get; set; } = "";
    public bool IsActive { get; set; } = true;

    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = [];
    public ICollection<DoctorSchedule> Schedules { get; set; } = [];
    public ICollection<LeavePeriod> LeavePeriods { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
