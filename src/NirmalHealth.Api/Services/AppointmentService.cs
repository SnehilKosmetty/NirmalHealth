using Microsoft.EntityFrameworkCore;
using NirmalHealth.Api.Models.Appointments;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;

namespace NirmalHealth.Api.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    private readonly SlotGenerationService _slotGen;

    public AppointmentService(AppDbContext db, SlotGenerationService slotGen)
    {
        _db = db;
        _slotGen = slotGen;
    }

    public async Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly from, DateOnly to, string? consultationType, CancellationToken ct = default)
    {
        var doctor = await _db.Doctors.Include(d => d.Hospital).FirstOrDefaultAsync(d => d.Id == doctorId && d.IsActive, ct);
        if (doctor == null) return Array.Empty<SlotDto>();

        await _slotGen.EnsureSlotsGeneratedAsync(doctorId, from, to, ct);

        var slots = await _db.AppointmentSlots
            .AsNoTracking()
            .Where(s => s.DoctorId == doctorId && !s.IsBooked && s.Date >= from && s.Date <= to)
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(consultationType) && Enum.TryParse<ConsultationType>(consultationType, true, out var ctEnum))
            slots = slots.Where(s => s.ConsultationType == ctEnum).ToList();

        return slots.Select(s => new SlotDto
        {
            Id = s.Id,
            DoctorId = doctorId,
            DoctorName = doctor.FullName,
            HospitalId = doctor.HospitalId,
            HospitalName = doctor.Hospital.Name,
            Date = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            ConsultationType = s.ConsultationType.ToString()
        });
    }

    public async Task<AppointmentDto?> BookAsync(int patientId, int slotId, string? chiefComplaint, CancellationToken ct = default)
    {
        var slot = await _db.AppointmentSlots
            .Include(s => s.Doctor).ThenInclude(d => d.Hospital)
            .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsBooked, ct);
        if (slot == null) return null;

        slot.IsBooked = true;
        var appointment = new Appointment
        {
            PatientId = patientId,
            SlotId = slot.Id,
            ChiefComplaint = chiefComplaint,
            Status = AppointmentStatus.Scheduled,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync(ct);

        var patient = await _db.Users.FindAsync(new object[] { patientId }, ct);
        return MapToDto(appointment, slot, patient);
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id, int? userId, string? role, int? hospitalId, CancellationToken ct = default)
    {
        var a = await _db.Appointments
            .Include(x => x.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a == null) return null;
        if (role != Core.Constants.RoleNames.SuperAdmin)
        {
            if (role == Core.Constants.RoleNames.Patient && a.PatientId != userId) return null;
            if (role == Core.Constants.RoleNames.HospitalAdmin && a.Slot.Doctor.HospitalId != hospitalId) return null;
        }
        return MapToDto(a, a.Slot, a.Patient);
    }

    public async Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(int patientId, CancellationToken ct = default)
    {
        var list = await _db.Appointments
            .Include(a => a.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(a => a.Patient)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.Slot.Date).ThenByDescending(a => a.Slot.StartTime)
            .ToListAsync(ct);
        return list.Select(a => MapToDto(a, a.Slot, a.Patient));
    }

    public async Task<IEnumerable<AppointmentDto>> GetHospitalAppointmentsAsync(int hospitalId, DateOnly? date, CancellationToken ct = default)
    {
        var q = _db.Appointments
            .Include(a => a.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(a => a.Patient)
            .Where(a => a.Slot.Doctor.HospitalId == hospitalId);
        if (date.HasValue) q = q.Where(a => a.Slot.Date == date.Value);
        var list = await q.OrderBy(a => a.Slot.Date).ThenBy(a => a.Slot.StartTime).ToListAsync(ct);
        return list.Select(a => MapToDto(a, a.Slot, a.Patient));
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync(DateOnly? date, CancellationToken ct = default)
    {
        var q = _db.Appointments
            .Include(a => a.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(a => a.Patient)
            .AsNoTracking();
        if (date.HasValue) q = q.Where(a => a.Slot.Date == date.Value);
        var list = await q.OrderBy(a => a.Slot.Date).ThenBy(a => a.Slot.StartTime).ToListAsync(ct);
        return list.Select(a => MapToDto(a, a.Slot, a.Patient));
    }

    public async Task<AppointmentDto?> CancelAsync(int appointmentId, int patientId, CancellationToken ct = default)
    {
        var a = await _db.Appointments
            .Include(x => x.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.PatientId == patientId, ct);
        if (a == null) return null;
        if (a.Status != AppointmentStatus.Scheduled) return null;

        a.Status = AppointmentStatus.Cancelled;
        a.UpdatedAtUtc = DateTime.UtcNow;
        var slot = await _db.AppointmentSlots.FindAsync(new object[] { a.SlotId }, ct);
        if (slot != null) slot.IsBooked = false;
        await _db.SaveChangesAsync(ct);
        return MapToDto(a, a.Slot, a.Patient);
    }

    public async Task<AppointmentDto?> UpdateChiefComplaintAsync(int appointmentId, int patientId, string? chiefComplaint, CancellationToken ct = default)
    {
        var a = await _db.Appointments
            .Include(x => x.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.PatientId == patientId, ct);
        if (a == null) return null;
        if (a.Status != AppointmentStatus.Scheduled) return null;

        a.ChiefComplaint = chiefComplaint;
        a.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return MapToDto(a, a.Slot, a.Patient);
    }

    public async Task<AppointmentDto?> RescheduleAsync(int appointmentId, int patientId, int newSlotId, string? chiefComplaint, CancellationToken ct = default)
    {
        var a = await _db.Appointments
            .Include(x => x.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.PatientId == patientId, ct);
        if (a == null) return null;
        if (a.Status != AppointmentStatus.Scheduled) return null;
        if (a.SlotId == newSlotId) return await UpdateChiefComplaintAsync(appointmentId, patientId, chiefComplaint, ct);

        var newSlot = await _db.AppointmentSlots
            .Include(s => s.Doctor).ThenInclude(d => d.Hospital)
            .FirstOrDefaultAsync(s => s.Id == newSlotId && !s.IsBooked, ct);
        if (newSlot == null) return null;

        var oldSlot = await _db.AppointmentSlots.FindAsync(new object[] { a.SlotId }, ct);
        if (oldSlot != null) oldSlot.IsBooked = false;
        newSlot.IsBooked = true;
        a.SlotId = newSlotId;
        a.ChiefComplaint = chiefComplaint ?? a.ChiefComplaint;
        a.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var updated = await _db.Appointments
            .Include(x => x.Slot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Hospital)
            .Include(x => x.Patient)
            .FirstAsync(x => x.Id == appointmentId, ct);
        return MapToDto(updated, updated.Slot, updated.Patient);
    }

    private static AppointmentDto MapToDto(Appointment a, AppointmentSlot slot, User? patient) => new()
    {
        Id = a.Id,
        SlotId = a.SlotId,
        Date = slot.Date,
        StartTime = slot.StartTime,
        ConsultationType = slot.ConsultationType.ToString(),
        ChiefComplaint = a.ChiefComplaint,
        Status = a.Status.ToString(),
        VideoMeetingUrl = a.VideoMeetingUrl,
        CreatedAtUtc = a.CreatedAtUtc,
        DoctorId = slot.DoctorId,
        DoctorName = slot.Doctor.FullName,
        HospitalId = slot.Doctor.HospitalId,
        HospitalName = slot.Doctor.Hospital.Name,
        PatientId = a.PatientId,
        PatientName = patient?.FullName ?? "",
        PatientPhone = patient?.Phone
    };
}
