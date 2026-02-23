namespace NirmalHealth.Api.Services;

public record SymptomAiResult(
    string UrgencyLevel,
    string Analysis,
    string SuggestedSpecialty,
    string[] PossibleConditions,
    string[] SuggestedActions
);

public interface IOpenAiService
{
    Task<SymptomAiResult> GetSymptomRecommendationAsync(string symptomsText, int? age, string? gender, string? medicalHistory, CancellationToken ct = default);
}
