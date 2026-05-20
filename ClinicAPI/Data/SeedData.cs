using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAPI.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ClinicDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        // Seed roles
        string[] roles = new[] { "Patient", "Doctor", "Receptionist", "ClinicManager" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // ── Clinic Manager ─────────────────────────────────────────────────
        await CreateUserAsync(userManager, new ApplicationUser
        {
            UserName = "manager@clinic.com",
            Email = "manager@clinic.com",
            FirstName = "Ahmed",
            LastName = "Al-Rashidi",
            Role = UserRole.ClinicManager,
            EmailConfirmed = true
        }, "Manager@123!", "ClinicManager");

        // ── Receptionist ───────────────────────────────────────────────────
        await CreateUserAsync(userManager, new ApplicationUser
        {
            UserName = "reception@clinic.com",
            Email = "reception@clinic.com",
            FirstName = "Sara",
            LastName = "Al-Mansoori",
            Role = UserRole.Receptionist,
            EmailConfirmed = true
        }, "Recept@123!", "Receptionist");

        // ── Doctors ────────────────────────────────────────────────────────
        var drKhalidUser = await CreateUserAsync(userManager, new ApplicationUser
        {
            UserName = "dr.khalid@clinic.com",
            Email = "dr.khalid@clinic.com",
            FirstName = "Khalid",
            LastName = "Al-Farsi",
            Role = UserRole.Doctor,
            EmailConfirmed = true
        }, "Doctor@123!", "Doctor");

        var drFatemaUser = await CreateUserAsync(userManager, new ApplicationUser
        {
            UserName = "dr.fatema@clinic.com",
            Email = "dr.fatema@clinic.com",
            FirstName = "Fatema",
            LastName = "Al-Zahra",
            Role = UserRole.Doctor,
            EmailConfirmed = true
        }, "Doctor@123!", "Doctor");

        // ── Patients ───────────────────────────────────────────────────────
        var patient1User = await CreateUserAsync(userManager, new ApplicationUser
        {
            UserName = "patient1@example.com",
            Email = "patient1@example.com",
            FirstName = "Mohammed",
            LastName = "Al-Khalifa",
            Role = UserRole.Patient,
            CPRNumber = "900112345",
            PatientReferenceNumber = "PAT-0001",
            EmailConfirmed = true
        }, "Patient@123!", "Patient");

        var patient2User = await CreateUserAsync(userManager, new ApplicationUser
        {
            UserName = "patient2@example.com",
            Email = "patient2@example.com",
            FirstName = "Aisha",
            LastName = "Al-Sayed",
            Role = UserRole.Patient,
            CPRNumber = "950267890",
            PatientReferenceNumber = "PAT-0002",
            EmailConfirmed = true
        }, "Patient@123!", "Patient");

        if (await db.Specializations.AnyAsync()) return; // already seeded

        // ── Specializations ────────────────────────────────────────────────
        var cardiology = new Specialization { Name = "Cardiology", Description = "Heart and cardiovascular system" };
        var pediatrics = new Specialization { Name = "Pediatrics", Description = "Medical care for children" };
        var general = new Specialization { Name = "General Practice", Description = "General medical care" };
        var dermatology = new Specialization { Name = "Dermatology", Description = "Skin conditions and disorders" };

        db.Specializations.AddRange(cardiology, pediatrics, general, dermatology);
        await db.SaveChangesAsync();

        // ── Doctor profiles ────────────────────────────────────────────────
        if (drKhalidUser != null && !await db.Doctors.AnyAsync(d => d.ApplicationUserId == drKhalidUser.Id))
        {
            var drKhalid = new Doctor
            {
                ApplicationUserId = drKhalidUser.Id,
                LicenseNumber = "BH-DOC-001",
                Bio = "Experienced cardiologist with 15 years of practice.",
                IsActive = true,
                DoctorSpecializations = new List<DoctorSpecialization>
                {
                    new DoctorSpecialization { SpecializationId = cardiology.Id },
                    new DoctorSpecialization { SpecializationId = general.Id }
                },
                Schedules = new List<DoctorSchedule>
                {
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Sunday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(14, 0, 0), SlotDurationMinutes = 30 },
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(14, 0, 0), SlotDurationMinutes = 30 },
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(14, 0, 0), SlotDurationMinutes = 30 },
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(14, 0, 0), SlotDurationMinutes = 30 },
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(14, 0, 0), SlotDurationMinutes = 30 }
                }
            };
            db.Doctors.Add(drKhalid);
        }

        if (drFatemaUser != null && !await db.Doctors.AnyAsync(d => d.ApplicationUserId == drFatemaUser.Id))
        {
            var drFatema = new Doctor
            {
                ApplicationUserId = drFatemaUser.Id,
                LicenseNumber = "BH-DOC-002",
                Bio = "Specialist in pediatrics and child development.",
                IsActive = true,
                DoctorSpecializations = new List<DoctorSpecialization>
                {
                    new DoctorSpecialization { SpecializationId = pediatrics.Id },
                    new DoctorSpecialization { SpecializationId = dermatology.Id }
                },
                Schedules = new List<DoctorSchedule>
                {
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Sunday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(16, 0, 0), SlotDurationMinutes = 30 },
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(16, 0, 0), SlotDurationMinutes = 30 },
                    new DoctorSchedule { DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(16, 0, 0), SlotDurationMinutes = 30 }
                }
            };
            db.Doctors.Add(drFatema);
        }

        // ── Patient profiles ───────────────────────────────────────────────
        if (patient1User != null && !await db.Patients.AnyAsync(p => p.ApplicationUserId == patient1User.Id))
            db.Patients.Add(new Patient { ApplicationUserId = patient1User.Id, CPRNumber = "900112345", PatientReferenceNumber = "PAT-0001", DateOfBirth = new DateTime(1990, 1, 1), Gender = "Male", Phone = "+973 3300 1111", Address = "Manama, Bahrain", BloodType = "O+" });

        if (patient2User != null && !await db.Patients.AnyAsync(p => p.ApplicationUserId == patient2User.Id))
            db.Patients.Add(new Patient { ApplicationUserId = patient2User.Id, CPRNumber = "950267890", PatientReferenceNumber = "PAT-0002", DateOfBirth = new DateTime(1995, 2, 6), Gender = "Female", Phone = "+973 3300 2222", Address = "Riffa, Bahrain", BloodType = "A+" });

        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string role)
    {
        if (await userManager.FindByEmailAsync(user.Email!) != null) return null;
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
        return result.Succeeded ? user : null;
    }
}
