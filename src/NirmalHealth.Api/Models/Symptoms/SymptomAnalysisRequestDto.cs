namespace NirmalHealth.Api.Models.Symptoms;

public class SymptomAnalysisRequestDto
{
    public string SymptomsText { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? MedicalHistory { get; set; }
}
