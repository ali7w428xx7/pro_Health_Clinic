IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [CPRNumber] nvarchar(450) NULL,
    [PatientReferenceNumber] nvarchar(450) NULL,
    [Role] nvarchar(max) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Specializations] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Specializations] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Doctors] (
    [Id] int NOT NULL IDENTITY,
    [ApplicationUserId] nvarchar(450) NOT NULL,
    [LicenseNumber] nvarchar(450) NOT NULL,
    [Bio] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Doctors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Doctors_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Patients] (
    [Id] int NOT NULL IDENTITY,
    [ApplicationUserId] nvarchar(450) NOT NULL,
    [CPRNumber] nvarchar(450) NOT NULL,
    [PatientReferenceNumber] nvarchar(450) NOT NULL,
    [DateOfBirth] datetime2 NOT NULL,
    [Gender] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [BloodType] nvarchar(max) NULL,
    [Allergies] nvarchar(max) NULL,
    CONSTRAINT [PK_Patients] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Patients_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DoctorSchedules] (
    [Id] int NOT NULL IDENTITY,
    [DoctorId] int NOT NULL,
    [DayOfWeek] int NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [SlotDurationMinutes] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_DoctorSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DoctorSchedules_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DoctorSpecializations] (
    [DoctorId] int NOT NULL,
    [SpecializationId] int NOT NULL,
    CONSTRAINT [PK_DoctorSpecializations] PRIMARY KEY ([DoctorId], [SpecializationId]),
    CONSTRAINT [FK_DoctorSpecializations_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DoctorSpecializations_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [LeavePeriods] (
    [Id] int NOT NULL IDENTITY,
    [DoctorId] int NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [IsApproved] bit NOT NULL,
    CONSTRAINT [PK_LeavePeriods] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeavePeriods_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Appointments] (
    [Id] int NOT NULL IDENTITY,
    [PatientId] int NOT NULL,
    [DoctorId] int NOT NULL,
    [SpecializationId] int NOT NULL,
    [AppointmentDate] date NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [ReasonForVisit] nvarchar(max) NOT NULL,
    [CancellationReason] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Appointments_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [NotificationType] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [RelatedAppointmentId] int NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Appointments_RelatedAppointmentId] FOREIGN KEY ([RelatedAppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [VisitRecords] (
    [Id] int NOT NULL IDENTITY,
    [AppointmentId] int NOT NULL,
    [DoctorNotes] nvarchar(max) NOT NULL,
    [Diagnosis] nvarchar(max) NOT NULL,
    [TreatmentPlan] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_VisitRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VisitRecords_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Prescriptions] (
    [Id] int NOT NULL IDENTITY,
    [VisitRecordId] int NOT NULL,
    [MedicationName] nvarchar(max) NOT NULL,
    [Dosage] nvarchar(max) NOT NULL,
    [Frequency] nvarchar(max) NOT NULL,
    [Duration] nvarchar(max) NOT NULL,
    [SpecialInstructions] nvarchar(max) NULL,
    [PrescribedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Prescriptions_VisitRecords_VisitRecordId] FOREIGN KEY ([VisitRecordId]) REFERENCES [VisitRecords] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Appointments_DoctorId] ON [Appointments] ([DoctorId]);

CREATE INDEX [IX_Appointments_PatientId] ON [Appointments] ([PatientId]);

CREATE INDEX [IX_Appointments_SpecializationId] ON [Appointments] ([SpecializationId]);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [IX_AspNetUsers_CPRNumber] ON [AspNetUsers] ([CPRNumber]) WHERE [CPRNumber] IS NOT NULL;

CREATE UNIQUE INDEX [IX_AspNetUsers_PatientReferenceNumber] ON [AspNetUsers] ([PatientReferenceNumber]) WHERE [PatientReferenceNumber] IS NOT NULL;

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Doctors_ApplicationUserId] ON [Doctors] ([ApplicationUserId]);

CREATE UNIQUE INDEX [IX_Doctors_LicenseNumber] ON [Doctors] ([LicenseNumber]);

CREATE INDEX [IX_DoctorSchedules_DoctorId] ON [DoctorSchedules] ([DoctorId]);

CREATE INDEX [IX_DoctorSpecializations_SpecializationId] ON [DoctorSpecializations] ([SpecializationId]);

CREATE INDEX [IX_LeavePeriods_DoctorId] ON [LeavePeriods] ([DoctorId]);

CREATE INDEX [IX_Notifications_RelatedAppointmentId] ON [Notifications] ([RelatedAppointmentId]);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

CREATE UNIQUE INDEX [IX_Patients_ApplicationUserId] ON [Patients] ([ApplicationUserId]);

CREATE UNIQUE INDEX [IX_Patients_CPRNumber] ON [Patients] ([CPRNumber]);

CREATE UNIQUE INDEX [IX_Patients_PatientReferenceNumber] ON [Patients] ([PatientReferenceNumber]);

CREATE INDEX [IX_Prescriptions_VisitRecordId] ON [Prescriptions] ([VisitRecordId]);

CREATE UNIQUE INDEX [IX_VisitRecords_AppointmentId] ON [VisitRecords] ([AppointmentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260512204734_InitialCreate', N'9.0.16');

COMMIT;
GO

