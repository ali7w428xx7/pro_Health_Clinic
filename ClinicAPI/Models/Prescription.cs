namespace ClinicAPI.Models;

public class Prescription
{
    public int Id { get; set; }
    public int VisitRecordId { get; set; }
    public VisitRecord VisitRecord { get; set; } = null!;
    public string MedicationName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Duration { get; set; } = "";
    public string? SpecialInstructions { get; set; }
    public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;
}
