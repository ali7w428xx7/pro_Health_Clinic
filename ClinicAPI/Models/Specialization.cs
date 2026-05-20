namespace ClinicAPI.Models;

public class Specialization
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}

public class DoctorSpecialization
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int SpecializationId { get; set; }
    public Specialization Specialization { get; set; } = null!;
}
