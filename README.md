# Nirmal Health

Healthcare app for Nirmal district, Telangana — find hospitals, AI symptom analysis, book appointments, emergency contacts.

## Stack

- **Backend:** .NET 8, ASP.NET Core Web API, SQL Server, EF Core, JWT
- **Frontend:** React 18, TypeScript, Vite, react-i18next (English, Hindi, Telugu)

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server (local SQL Express or remote)

## Setup

### 1. Database

Create a database named `NirmalHealth` on your SQL Server instance and set the connection string in:

- `src/NirmalHealth.Api/appsettings.json` (or `appsettings.Development.json`)

Default (Windows Authentication, local SQL Express):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SNEHILKOSMETTY\\SQLEXPRESS;Database=NirmalHealth;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

If you use SQL Server authentication, use `User Id=...;Password=...` instead of `Trusted_Connection=True`.

### 2. Backend

```bash
cd src/NirmalHealth.Api
dotnet run
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

On first run, EF Core will apply migrations and seed data (roles, 5 hospitals, 25 doctors, slot templates, super-admin user).

**Default super-admin:** `admin@nirmalhealth.in` / `Admin@123`

### 3. Frontend

```bash
cd client
npm install
npm run dev
```

- App: http://localhost:5173  
- API requests are proxied to http://localhost:5000 (see `client/vite.config.ts`).

## Optional configuration

- **OpenAI (symptom analysis):** Set `OpenAI:ApiKey` in `appsettings.json`. If empty, the symptom checker returns a default suggestion.
- **JWT:** Change `Jwt:Key` in production (min 32 characters).

## Features

- **Public:** Home, Hospitals (search, filters, Find Near Me), Symptom Checker (AI), Emergency (108, 100, 101, 181, 1098, nearest hospitals)
- **Patient:** Register, Login, Book appointment (by doctor), My Appointments
- **Hospital admin:** View/edit own hospital, doctors, view appointments for own hospital
- **Super-admin:** Create hospitals, create hospital-admin users, view all hospitals/doctors/appointments

## License

Private / All rights reserved.
