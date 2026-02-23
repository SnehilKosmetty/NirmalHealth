namespace NirmalHealth.Api.Models.Symptoms;

public class SymptomAnalysisResultDto
{
    /// <summary>e.g. Urgent, Moderate, Routine</summary>
    public string UrgencyLevel { get; set; } = "Routine";
    /// <summary>Detailed analysis paragraph for the patient (possible causes, context).</summary>
    public string Analysis { get; set; } = string.Empty;
    public string SuggestedSpecialty { get; set; } = string.Empty;
    public string[] PossibleConditions { get; set; } = Array.Empty<string>();
    /// <summary>Numbered self-care and when-to-seek-care actions (5-7 items).</summary>
    public string[] SuggestedActions { get; set; } = Array.Empty<string>();
    public string Disclaimer { get; set; } = "This AI analysis is for informational purposes only and should not replace professional medical advice. Always consult a qualified healthcare provider for diagnosis and treatment.";
    public int[] RecommendedHospitalIds { get; set; } = Array.Empty<int>();
}
