using Microsoft.EntityFrameworkCore;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;

namespace NirmalHealth.Api.Services;

/// <summary>Generates concrete AppointmentSlots from SlotTemplates for a date range.</summary>
public class SlotGenerationService
{
    private readonly AppDbContext _db;

    public SlotGenerationService(AppDbContext db) => _db = db;

    public async Task EnsureSlotsGeneratedAsync(int doctorId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var templates = await _db.SlotTemplates
            .Where(t => t.DoctorId == doctorId && t.IsActive)
            .ToListAsync(ct);
        if (templates.Count == 0) return;

        var existing = await _db.AppointmentSlots
            .Where(s => s.DoctorId == doctorId && s.Date >= from && s.Date <= to)
            .Select(s => new { s.Date, s.StartTime })
            .ToListAsync(ct);
        var existingSet = existing.Select(x => (x.Date, x.StartTime)).ToHashSet();

        var toAdd = new List<AppointmentSlot>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            foreach (var t in templates.Where(t => t.DayOfWeek == d.DayOfWeek))
            {
                for (var start = t.StartTime; start < t.EndTime; start = start.Add(TimeSpan.FromMinutes(t.DurationMinutes)))
                {
                    if (existingSet.Contains((d, start))) continue;
                    toAdd.Add(new AppointmentSlot
                    {
                        DoctorId = doctorId,
                        Date = d,
                        StartTime = start,
                        EndTime = start.Add(TimeSpan.FromMinutes(t.DurationMinutes)),
                        ConsultationType = t.ConsultationType,
                        IsBooked = false,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                    existingSet.Add((d, start));
                }
            }
        }

        if (toAdd.Count > 0)
        {
            await _db.AppointmentSlots.AddRangeAsync(toAdd, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}
