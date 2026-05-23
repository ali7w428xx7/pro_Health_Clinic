using System.ComponentModel.DataAnnotations;
using ClinicAPI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicMVC.ViewModels;

public class BookAppointmentViewModel
{
    [Required, Display(Name = "Specialization")]
    public int SpecializationId { get; set; }

    [Required, Display(Name = "Doctor")]
    public int DoctorId { get; set; }

    [Required, Display(Name = "Appointment Date"), DataType(DataType.Date)]
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [Required, Display(Name = "Time Slot")]
    public string TimeSlot { get; set; } = "";

    [Required, Display(Name = "Reason for Visit"), MaxLength(500)]
    public string ReasonForVisit { get; set; } = "";

    // For receptionist booking on behalf of patient
    public int? PatientId { get; set; }

    public List<SelectListItem> Specializations { get; set; } = [];
    public List<SelectListItem> Doctors { get; set; } = [];
    public List<SelectListItem> TimeSlots { get; set; } = [];
    public List<SelectListItem> Patients { get; set; } = [];
}

public class AppointmentListViewModel
{
    public List<AppointmentRowViewModel> Appointments { get; set; } = [];
    public string? StatusFilter { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

public class AppointmentRowViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = "";
    public string PatientCPR { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string SpecializationName { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string ReasonForVisit { get; set; } = "";
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentDetailViewModel : AppointmentRowViewModel
{
    public VisitRecordViewModel? VisitRecord { get; set; }
    public List<PrescriptionViewModel> Prescriptions { get; set; } = [];
}

public class UpdateStatusViewModel
{
    public int AppointmentId { get; set; }
    public AppointmentStatus NewStatus { get; set; }
    public string? CancellationReason { get; set; }
    public string? DoctorNotes { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
}

public class VisitRecordViewModel
{
    public int Id { get; set; }
    public string DoctorNotes { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string TreatmentPlan { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class CreateVisitRecordViewModel
{
    public int AppointmentId { get; set; }

    [Required, Display(Name = "Doctor Notes")]
    public string DoctorNotes { get; set; } = "";

    [Required]
    public string Diagnosis { get; set; } = "";

    [Required, Display(Name = "Treatment Plan")]
    public string TreatmentPlan { get; set; } = "";
}

public class PrescriptionViewModel
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Duration { get; set; } = "";
    public string? SpecialInstructions { get; set; }
    public DateTime PrescribedAt { get; set; }
}

public class AddPrescriptionViewModel
{
    public int VisitRecordId { get; set; }

    [Required, Display(Name = "Medication Name")]
    public string MedicationName { get; set; } = "";

    [Required]
    public string Dosage { get; set; } = "";

    [Required]
    public string Frequency { get; set; } = "";

    [Required]
    public string Duration { get; set; } = "";

    [Display(Name = "Special Instructions")]
    public string? SpecialInstructions { get; set; }
}

public class CancelAppointmentViewModel
{
    public int AppointmentId { get; set; }

    [Required, Display(Name = "Reason for Cancellation")]
    public string CancellationReason { get; set; } = "";
}
