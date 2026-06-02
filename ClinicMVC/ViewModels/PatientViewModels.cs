using System.ComponentModel.DataAnnotations;
namespace ClinicMVC.ViewModels;

public class PatientProfileViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string CPRNumber { get; set; } = "";
    public string PatientReferenceNumber { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public List<AppointmentRowViewModel> RecentAppointments { get; set; } = [];
    public List<VisitHistoryViewModel> VisitHistory { get; set; } = [];
}

public class VisitHistoryViewModel
{
    public int AppointmentId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public string DoctorName { get; set; } = "";
    public string SpecializationName { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string TreatmentPlan { get; set; } = "";
    public string DoctorNotes { get; set; } = "";
    public List<PrescriptionViewModel> Prescriptions { get; set; } = [];
}

public class PatientListViewModel
{
    public List<PatientSummaryViewModel> Patients { get; set; } = [];
    public string? Search { get; set; }
}

public class PatientSummaryViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string CPRNumber { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Gender { get; set; } = "";
    public int AppointmentCount { get; set; }
    public DateOnly? LastVisit { get; set; }
}

public class PatientEditProfileViewModel
{
    public int Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, Display(Name = "Date of Birth"), DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required, Phone]
    public string Phone { get; set; } = "";

    [Required]
    public string Address { get; set; } = "";
}
