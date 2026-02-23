using NirmalHealth.Api.Models.Hospitals;

namespace NirmalHealth.Api.Services;

public interface IHospitalService
{
    Task<IEnumerable<HospitalListDto>> GetHospitalsAsync(HospitalFilterDto? filter, CancellationToken ct = default);
    Task<HospitalDetailDto?> GetHospitalByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<HospitalListDto>> GetNearestHospitalsAsync(double? lat, double? lon, int count = 10, bool emergencyOnly = false, CancellationToken ct = default);
}
