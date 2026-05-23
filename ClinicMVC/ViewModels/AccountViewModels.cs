using System.ComponentModel.DataAnnotations;
using ClinicAPI.Models;

namespace ClinicMVC.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class RegisterPatientViewModel
{
    [Required, Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [Required, Display(Name = "Last Name")]
    public string LastName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = "";

    [Required, Display(Name = "CPR Number")]
    public string CPRNumber { get; set; } = "";

    [Required, Display(Name = "Date of Birth"), DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string Gender { get; set; } = "";

    [Required, Phone]
    public string Phone { get; set; } = "";

    [Required]
    public string Address { get; set; } = "";

    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
}

public class CreateStaffViewModel
{
    [Required, Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [Required, Display(Name = "Last Name")]
    public string LastName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    public UserRole Role { get; set; }

    // Doctor-only fields
    public string? LicenseNumber { get; set; }
    public string? Bio { get; set; }
    public List<int> SpecializationIds { get; set; } = [];
}
