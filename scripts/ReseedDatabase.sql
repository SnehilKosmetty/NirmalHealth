-- Run this in SQL Server Management Studio to clear all data and allow re-seed.
-- After running: start the API (dotnet run). SeedAsync will see empty Roles and insert 40 hospitals, 80 doctors, etc.
-- Keeps __EFMigrationsHistory so migrations are NOT re-applied (tables stay).

USE NirmalHealth;
GO

-- Delete in dependency order (children first; FKs stay enabled)
DELETE FROM [AuditLogs];
DELETE FROM [SymptomAnalyses];
DELETE FROM [Appointments];
DELETE FROM [AppointmentSlots];
DELETE FROM [SlotTemplates];
DELETE FROM [DoctorSpecialties];
DELETE FROM [HospitalSpecialties];
DELETE FROM [UserRoles];
DELETE FROM [Doctors];
DELETE FROM [Users];
DELETE FROM [Hospitals];
DELETE FROM [Specialties];
DELETE FROM [Roles];
GO

-- Reseed identity columns so new rows get IDs starting from 1 (seed uses IDENTITY_INSERT with explicit IDs)
DBCC CHECKIDENT ('Roles', RESEED, 0);
DBCC CHECKIDENT ('Specialties', RESEED, 0);
DBCC CHECKIDENT ('Hospitals', RESEED, 0);
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Doctors', RESEED, 0);
GO

PRINT 'Database cleared. Start the API (dotnet run) to re-seed 40 hospitals and all data.';
