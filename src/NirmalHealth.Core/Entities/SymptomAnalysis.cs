namespace NirmalHealth.Core.Entities;

/// <summary>Log of AI symptom analysis (server-side only).</summary>
public class SymptomAnalysis
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string SymptomsText { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? MedicalHistory { get; set; }
    public string SuggestedSpecialty { get; set; } = string.Empty;
    public string? PossibleConditionsJson { get; set; }
    public string? RecommendedHospitalIdsJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
