using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicMVC.ViewModels;

public class DoctorListViewModel
{
    public List<DoctorSummaryViewModel> Doctors { get; set; } = [];
}

public class DoctorSummaryViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public string Bio { get; set; } = "";
    public bool IsActive { get; set; }
    public List<string> Specializations { get; set; } = [];
    public int TotalAppointments { get; set; }
}

public class DoctorDetailViewModel : DoctorSummaryViewModel
{
    public List<ScheduleViewModel> Schedules { get; set; } = [];
    public List<LeaveViewModel> LeavePeriods { get; set; } = [];
}

public class EditDoctorViewModel
{
    public int Id { get; set; }

    [Required]
    public string Bio { get; set; } = "";

    public bool IsActive { get; set; }
    public List<int> SpecializationIds { get; set; } = [];
    public List<SelectListItem> AllSpecializations { get; set; } = [];
}

public class ScheduleViewModel
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class AddScheduleViewModel
{
    public int DoctorId { get; set; }

    [Required, Display(Name = "Day of Week")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required, Display(Name = "Start Time"), DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [Required, Display(Name = "End Time"), DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }

    [Required, Range(15, 120), Display(Name = "Slot Duration (minutes)")]
    public int SlotDurationMinutes { get; set; } = 30;
}

public class LeaveViewModel
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Reason { get; set; } = "";
    public bool IsApproved { get; set; }
}

public class AddLeaveViewModel
{
    public int DoctorId { get; set; }

    [Required, Display(Name = "Start Date"), DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }

    [Required, Display(Name = "End Date"), DataType(DataType.Date)]
    public DateOnly EndDate { get; set; }

    [Required]
    public string Reason { get; set; } = "";
}

public class SpecializationViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int DoctorCount { get; set; }
}

public class CreateSpecializationViewModel
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string Description { get; set; } = "";
}
