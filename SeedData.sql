-- ============================================================
-- ClinicSystem Seed Data Script
-- Run this in SSMS against ClinicSystemDB
-- ============================================================

USE ClinicSystemDB;
GO

-- ============================================================
-- 1. ROLES
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE [Name] = 'Patient')
INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
VALUES ('R0000001-0000-0000-0000-000000000001', 'Patient', 'PATIENT', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE [Name] = 'Doctor')
INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
VALUES ('R0000002-0000-0000-0000-000000000002', 'Doctor', 'DOCTOR', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE [Name] = 'Receptionist')
INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
VALUES ('R0000003-0000-0000-0000-000000000003', 'Receptionist', 'RECEPTIONIST', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE [Name] = 'ClinicManager')
INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
VALUES ('R0000004-0000-0000-0000-000000000004', 'ClinicManager', 'CLINICMANAGER', NEWID());

-- ============================================================
-- 2. USERS  (Role enum: Patient=0, Doctor=1, Receptionist=2, ClinicManager=3)
-- ============================================================

-- Clinic Manager
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'manager@clinic.com')
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, [Role], CPRNumber, PatientReferenceNumber)
VALUES (
    'U0000001-0000-0000-0000-000000000001',
    'manager@clinic.com', 'MANAGER@CLINIC.COM',
    'manager@clinic.com', 'MANAGER@CLINIC.COM',
    1,
    'AQAAAAEAACcQAAAAEJvdBx+/ARM2zK7ji/LQeMuuyt4dMHz1tDNRxP1VHiTyqFattPlaotu26NJRawwGlg==',
    NEWID(), NEWID(),
    0, 0, 1, 0,
    'Ahmed', 'Al-Rashidi', 3, NULL, NULL);

-- Receptionist
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'reception@clinic.com')
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, [Role], CPRNumber, PatientReferenceNumber)
VALUES (
    'U0000002-0000-0000-0000-000000000002',
    'reception@clinic.com', 'RECEPTION@CLINIC.COM',
    'reception@clinic.com', 'RECEPTION@CLINIC.COM',
    1,
    'AQAAAAEAACcQAAAAEGOblnQfdJkRk9BoBfRRBw2eo2WiMvXm6Dcl1o4HAGkVAwvfyF+3DGPG7TDmwdrakQ==',
    NEWID(), NEWID(),
    0, 0, 1, 0,
    'Sara', 'Al-Mansoori', 2, NULL, NULL);

-- Doctor: Khalid
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'dr.khalid@clinic.com')
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, [Role], CPRNumber, PatientReferenceNumber)
VALUES (
    'U0000003-0000-0000-0000-000000000003',
    'dr.khalid@clinic.com', 'DR.KHALID@CLINIC.COM',
    'dr.khalid@clinic.com', 'DR.KHALID@CLINIC.COM',
    1,
    'AQAAAAEAACcQAAAAEPdyYJ03Qz0BhomgaVWs8DC0M/hquscE/6+5JbiWUjZCJqz2cPCn7hxND9I76ZCyBg==',
    NEWID(), NEWID(),
    0, 0, 1, 0,
    'Khalid', 'Al-Farsi', 1, NULL, NULL);

-- Doctor: Fatema
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'dr.fatema@clinic.com')
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, [Role], CPRNumber, PatientReferenceNumber)
VALUES (
    'U0000004-0000-0000-0000-000000000004',
    'dr.fatema@clinic.com', 'DR.FATEMA@CLINIC.COM',
    'dr.fatema@clinic.com', 'DR.FATEMA@CLINIC.COM',
    1,
    'AQAAAAEAACcQAAAAEPdyYJ03Qz0BhomgaVWs8DC0M/hquscE/6+5JbiWUjZCJqz2cPCn7hxND9I76ZCyBg==',
    NEWID(), NEWID(),
    0, 0, 1, 0,
    'Fatema', 'Al-Zahra', 1, NULL, NULL);

-- Patient 1
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'patient1@example.com')
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, [Role], CPRNumber, PatientReferenceNumber)
VALUES (
    'U0000005-0000-0000-0000-000000000005',
    'patient1@example.com', 'PATIENT1@EXAMPLE.COM',
    'patient1@example.com', 'PATIENT1@EXAMPLE.COM',
    1,
    'AQAAAAEAACcQAAAAEJ4CzdFlwqK63ElKynQW9GyR+s+Z7xblSFYbDnnmka3MZeNK7GA6hD/jBLdZLX3i9g==',
    NEWID(), NEWID(),
    0, 0, 1, 0,
    'Mohammed', 'Al-Khalifa', 0, '900112345', 'PAT-0001');

-- Patient 2
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'patient2@example.com')
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, [Role], CPRNumber, PatientReferenceNumber)
VALUES (
    'U0000006-0000-0000-0000-000000000006',
    'patient2@example.com', 'PATIENT2@EXAMPLE.COM',
    'patient2@example.com', 'PATIENT2@EXAMPLE.COM',
    1,
    'AQAAAAEAACcQAAAAEJ4CzdFlwqK63ElKynQW9GyR+s+Z7xblSFYbDnnmka3MZeNK7GA6hD/jBLdZLX3i9g==',
    NEWID(), NEWID(),
    0, 0, 1, 0,
    'Aisha', 'Al-Sayed', 0, '950267890', 'PAT-0002');

-- ============================================================
-- 3. USER ROLES
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'U0000001-0000-0000-0000-000000000001')
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('U0000001-0000-0000-0000-000000000001', 'R0000004-0000-0000-0000-000000000004');

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'U0000002-0000-0000-0000-000000000002')
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('U0000002-0000-0000-0000-000000000002', 'R0000003-0000-0000-0000-000000000003');

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'U0000003-0000-0000-0000-000000000003')
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('U0000003-0000-0000-0000-000000000003', 'R0000002-0000-0000-0000-000000000002');

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'U0000004-0000-0000-0000-000000000004')
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('U0000004-0000-0000-0000-000000000004', 'R0000002-0000-0000-0000-000000000002');

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'U0000005-0000-0000-0000-000000000005')
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('U0000005-0000-0000-0000-000000000005', 'R0000001-0000-0000-0000-000000000001');

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'U0000006-0000-0000-0000-000000000006')
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('U0000006-0000-0000-0000-000000000006', 'R0000001-0000-0000-0000-000000000001');

-- ============================================================
-- 4. SPECIALIZATIONS
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Specializations WHERE [Name] = 'Cardiology')
INSERT INTO Specializations ([Name], [Description])
VALUES ('Cardiology', 'Heart and cardiovascular system');

IF NOT EXISTS (SELECT 1 FROM Specializations WHERE [Name] = 'Pediatrics')
INSERT INTO Specializations ([Name], [Description])
VALUES ('Pediatrics', 'Medical care for children');

IF NOT EXISTS (SELECT 1 FROM Specializations WHERE [Name] = 'General Practice')
INSERT INTO Specializations ([Name], [Description])
VALUES ('General Practice', 'General medical care');

IF NOT EXISTS (SELECT 1 FROM Specializations WHERE [Name] = 'Dermatology')
INSERT INTO Specializations ([Name], [Description])
VALUES ('Dermatology', 'Skin conditions and disorders');

-- ============================================================
-- 5. DOCTOR PROFILES
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Doctors WHERE ApplicationUserId = 'U0000003-0000-0000-0000-000000000003')
INSERT INTO Doctors (ApplicationUserId, LicenseNumber, Bio, IsActive)
VALUES ('U0000003-0000-0000-0000-000000000003', 'BH-DOC-001',
        'Experienced cardiologist with 15 years of practice.', 1);

IF NOT EXISTS (SELECT 1 FROM Doctors WHERE ApplicationUserId = 'U0000004-0000-0000-0000-000000000004')
INSERT INTO Doctors (ApplicationUserId, LicenseNumber, Bio, IsActive)
VALUES ('U0000004-0000-0000-0000-000000000004', 'BH-DOC-002',
        'Specialist in pediatrics and child development.', 1);

-- ============================================================
-- 6. DOCTOR SPECIALIZATIONS
-- ============================================================
-- Dr. Khalid → Cardiology + General Practice
IF NOT EXISTS (
    SELECT 1 FROM DoctorSpecializations ds
    JOIN Doctors d ON ds.DoctorId = d.Id
    JOIN Specializations s ON ds.SpecializationId = s.Id
    WHERE d.ApplicationUserId = 'U0000003-0000-0000-0000-000000000003'
    AND s.[Name] = 'Cardiology')
INSERT INTO DoctorSpecializations (DoctorId, SpecializationId)
SELECT d.Id, s.Id
FROM Doctors d, Specializations s
WHERE d.ApplicationUserId = 'U0000003-0000-0000-0000-000000000003'
AND s.[Name] = 'Cardiology';

IF NOT EXISTS (
    SELECT 1 FROM DoctorSpecializations ds
    JOIN Doctors d ON ds.DoctorId = d.Id
    JOIN Specializations s ON ds.SpecializationId = s.Id
    WHERE d.ApplicationUserId = 'U0000003-0000-0000-0000-000000000003'
    AND s.[Name] = 'General Practice')
INSERT INTO DoctorSpecializations (DoctorId, SpecializationId)
SELECT d.Id, s.Id
FROM Doctors d, Specializations s
WHERE d.ApplicationUserId = 'U0000003-0000-0000-0000-000000000003'
AND s.[Name] = 'General Practice';

-- Dr. Fatema → Pediatrics + Dermatology
IF NOT EXISTS (
    SELECT 1 FROM DoctorSpecializations ds
    JOIN Doctors d ON ds.DoctorId = d.Id
    JOIN Specializations s ON ds.SpecializationId = s.Id
    WHERE d.ApplicationUserId = 'U0000004-0000-0000-0000-000000000004'
    AND s.[Name] = 'Pediatrics')
INSERT INTO DoctorSpecializations (DoctorId, SpecializationId)
SELECT d.Id, s.Id
FROM Doctors d, Specializations s
WHERE d.ApplicationUserId = 'U0000004-0000-0000-0000-000000000004'
AND s.[Name] = 'Pediatrics';

IF NOT EXISTS (
    SELECT 1 FROM DoctorSpecializations ds
    JOIN Doctors d ON ds.DoctorId = d.Id
    JOIN Specializations s ON ds.SpecializationId = s.Id
    WHERE d.ApplicationUserId = 'U0000004-0000-0000-0000-000000000004'
    AND s.[Name] = 'Dermatology')
INSERT INTO DoctorSpecializations (DoctorId, SpecializationId)
SELECT d.Id, s.Id
FROM Doctors d, Specializations s
WHERE d.ApplicationUserId = 'U0000004-0000-0000-0000-000000000004'
AND s.[Name] = 'Dermatology';

-- ============================================================
-- 7. DOCTOR SCHEDULES (DayOfWeek: Sun=0,Mon=1,Tue=2,Wed=3,Thu=4)
-- ============================================================
-- Dr. Khalid: Sun-Thu 08:00-14:00, 30-min slots
INSERT INTO DoctorSchedules (DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes, IsActive)
SELECT d.Id, day.DayNum, '08:00:00', '14:00:00', 30, 1
FROM Doctors d
CROSS JOIN (VALUES (0),(1),(2),(3),(4)) AS day(DayNum)
WHERE d.ApplicationUserId = 'U0000003-0000-0000-0000-000000000003'
AND NOT EXISTS (
    SELECT 1 FROM DoctorSchedules ds2
    WHERE ds2.DoctorId = d.Id AND ds2.DayOfWeek = day.DayNum);

-- Dr. Fatema: Sun, Tue, Thu 10:00-16:00, 30-min slots
INSERT INTO DoctorSchedules (DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes, IsActive)
SELECT d.Id, day.DayNum, '10:00:00', '16:00:00', 30, 1
FROM Doctors d
CROSS JOIN (VALUES (0),(2),(4)) AS day(DayNum)
WHERE d.ApplicationUserId = 'U0000004-0000-0000-0000-000000000004'
AND NOT EXISTS (
    SELECT 1 FROM DoctorSchedules ds2
    WHERE ds2.DoctorId = d.Id AND ds2.DayOfWeek = day.DayNum);

-- ============================================================
-- 8. PATIENT PROFILES
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Patients WHERE ApplicationUserId = 'U0000005-0000-0000-0000-000000000005')
INSERT INTO Patients (ApplicationUserId, CPRNumber, PatientReferenceNumber,
                      DateOfBirth, Gender, Phone, [Address], BloodType, Allergies)
VALUES ('U0000005-0000-0000-0000-000000000005', '900112345', 'PAT-0001',
        '1990-01-01', 'Male', '+973 3300 1111', 'Manama, Bahrain', 'O+', NULL);

IF NOT EXISTS (SELECT 1 FROM Patients WHERE ApplicationUserId = 'U0000006-0000-0000-0000-000000000006')
INSERT INTO Patients (ApplicationUserId, CPRNumber, PatientReferenceNumber,
                      DateOfBirth, Gender, Phone, [Address], BloodType, Allergies)
VALUES ('U0000006-0000-0000-0000-000000000006', '950267890', 'PAT-0002',
        '1995-02-06', 'Female', '+973 3300 2222', 'Riffa, Bahrain', 'A+', NULL);

-- ============================================================
-- VERIFY
-- ============================================================
SELECT 'Users' AS [Table], COUNT(*) AS [Count] FROM AspNetUsers
UNION ALL SELECT 'Roles', COUNT(*) FROM AspNetRoles
UNION ALL SELECT 'UserRoles', COUNT(*) FROM AspNetUserRoles
UNION ALL SELECT 'Specializations', COUNT(*) FROM Specializations
UNION ALL SELECT 'Doctors', COUNT(*) FROM Doctors
UNION ALL SELECT 'DoctorSchedules', COUNT(*) FROM DoctorSchedules
UNION ALL SELECT 'Patients', COUNT(*) FROM Patients;
GO
