namespace NirmalHealth.Api.Models.Hospitals;

public class HospitalListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int BedCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsEmergency { get; set; }
    public string[] Specialties { get; set; } = Array.Empty<string>();
    public double? DistanceKm { get; set; }
}
