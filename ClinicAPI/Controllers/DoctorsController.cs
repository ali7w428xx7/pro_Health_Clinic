using ClinicAPI.Data;
using ClinicAPI.DTOs;
using ClinicAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly ClinicDbContext _db;
    private readonly IAppointmentService _apptService;

    public DoctorsController(ClinicDbContext db, IAppointmentService apptService)
    {
        _db = db;
        _apptService = apptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .Include(d => d.DoctorSpecializations).ThenInclude(ds => ds.Specialization)
            .Where(d => d.IsActive)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                ApplicationUserId = d.ApplicationUserId,
                FullName = d.ApplicationUser.FullName,
                Email = d.ApplicationUser.Email!,
                LicenseNumber = d.LicenseNumber,
                Bio = d.Bio,
                IsActive = d.IsActive,
                Specializations = d.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList()
            })
            .ToListAsync();

        return Ok(doctors);
    }

    [HttpGet("{id}/availability")]
    public async Task<IActionResult> GetAvailability(int id, [FromQuery] DateOnly date)
    {
        var doctor = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null) return NotFound();

        var slots = await _apptService.GetAvailableSlotsAsync(id, date);

        return Ok(new DoctorAvailabilityDto
        {
            DoctorId = id,
            DoctorName = doctor.ApplicationUser.FullName,
            Date = date,
            AvailableSlots = slots
        });
    }
}
