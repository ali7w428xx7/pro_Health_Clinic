namespace ClinicAPI.Models;

public class Patient
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = "";
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public string CPRNumber { get; set; } = "";
    public string PatientReferenceNumber { get; set; } = Guid.NewGuid().ToString();
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];
}
