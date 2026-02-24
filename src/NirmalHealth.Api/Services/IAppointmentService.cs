using NirmalHealth.Api.Models.Appointments;

namespace NirmalHealth.Api.Services;

public interface IAppointmentService
{
    Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly from, DateOnly to, string? consultationType, CancellationToken ct = default);
    Task<AppointmentDto?> BookAsync(int patientId, int slotId, string? chiefComplaint, CancellationToken ct = default);
    Task<AppointmentDto?> GetAppointmentByIdAsync(int id, int? userId, string? role, int? hospitalId, CancellationToken ct = default);
    Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(int patientId, CancellationToken ct = default);
    Task<IEnumerable<AppointmentDto>> GetHospitalAppointmentsAsync(int hospitalId, DateOnly? date, CancellationToken ct = default);
    Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync(DateOnly? date, CancellationToken ct = default);
    Task<AppointmentDto?> CancelAsync(int appointmentId, int patientId, CancellationToken ct = default);
    Task<AppointmentDto?> UpdateChiefComplaintAsync(int appointmentId, int patientId, string? chiefComplaint, CancellationToken ct = default);
    Task<AppointmentDto?> RescheduleAsync(int appointmentId, int patientId, int newSlotId, string? chiefComplaint, CancellationToken ct = default);
}
