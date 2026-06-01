using Microsoft.AspNetCore.Identity;

namespace ClinicAPI.Models;

public enum UserRole { Patient, Doctor, Receptionist, ClinicManager }

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? CPRNumber { get; set; }
    public string? PatientReferenceNumber { get; set; }
    public UserRole Role { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public ICollection<Notification> Notifications { get; set; } = [];
}
