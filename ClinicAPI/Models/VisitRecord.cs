namespace ClinicAPI.Models;

public class VisitRecord
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string DoctorNotes { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string TreatmentPlan { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
