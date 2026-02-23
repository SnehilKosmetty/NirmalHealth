using NirmalHealth.Api.Models.Doctors;

namespace NirmalHealth.Api.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorListDto>> GetDoctorsByHospitalAsync(int hospitalId, CancellationToken ct = default);
    Task<DoctorDetailDto?> GetDoctorByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<DoctorListDto>> GetDoctorsAsync(int? hospitalId, string? specialty, CancellationToken ct = default);
}
