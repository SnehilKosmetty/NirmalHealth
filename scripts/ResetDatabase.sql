-- Run this in SSMS when you get "Invalid object name 'Roles'" after "No migrations were applied".
-- It clears migration history so the next app run will re-apply the Initial migration and create all tables.

USE NirmalHealth;
GO

-- Remove migration history so EF Core will re-apply the Initial migration on next startup
DELETE FROM [__EFMigrationsHistory];
GO

-- Then stop the app (if running) and run: dotnet run
-- The migration will run again and create Roles, Users, Hospitals, etc.
