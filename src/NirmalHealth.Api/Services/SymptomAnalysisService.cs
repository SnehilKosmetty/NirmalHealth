using Microsoft.EntityFrameworkCore;
using NirmalHealth.Api.Models.Symptoms;
using NirmalHealth.Core.Entities;
using NirmalHealth.Infrastructure.Data;

namespace NirmalHealth.Api.Services;

public class SymptomAnalysisService : ISymptomAnalysisService
{
    private readonly AppDbContext _db;
    private readonly IOpenAiService _openAi;

    public SymptomAnalysisService(AppDbContext db, IOpenAiService openAi)
    {
        _db = db;
        _openAi = openAi;
    }

    public async Task<SymptomAnalysisResultDto> AnalyzeAsync(SymptomAnalysisRequestDto dto, int? userId, CancellationToken ct = default)
    {
        var ai = await _openAi.GetSymptomRecommendationAsync(
            dto.SymptomsText, dto.Age, dto.Gender, dto.MedicalHistory, ct);

        var suggestedSpecialty = ai.SuggestedSpecialty ?? "General Medicine";
        var specialtyForDb = suggestedSpecialty.Replace("General Physician / Internal Medicine", "General Medicine").Trim();
        if (string.IsNullOrWhiteSpace(specialtyForDb)) specialtyForDb = "General Medicine";
        var hospitals = await _db.Hospitals
            .AsNoTracking()
            .Include(h => h.HospitalSpecialties).ThenInclude(hs => hs.Specialty)
            .Where(h => h.IsActive && h.HospitalSpecialties.Any(hs => hs.Specialty != null && (hs.Specialty.Name == specialtyForDb || hs.Specialty.Name == suggestedSpecialty)))
            .Select(h => h.Id)
            .Take(10)
            .ToListAsync(ct);
        if (hospitals.Count == 0)
            hospitals = await _db.Hospitals.Where(h => h.IsActive).Select(h => h.Id).Take(5).ToListAsync(ct);

        var log = new SymptomAnalysis
        {
            UserId = userId,
            SymptomsText = dto.SymptomsText ?? "",
            Age = dto.Age,
            Gender = dto.Gender,
            MedicalHistory = dto.MedicalHistory,
            SuggestedSpecialty = specialtyForDb,
            PossibleConditionsJson = System.Text.Json.JsonSerializer.Serialize(ai.PossibleConditions ?? Array.Empty<string>()),
            RecommendedHospitalIdsJson = System.Text.Json.JsonSerializer.Serialize(hospitals),
            CreatedAtUtc = DateTime.UtcNow
        };
        try
        {
            _db.SymptomAnalyses.Add(log);
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // Log save is best-effort; still return the analysis to the user
        }

        return new SymptomAnalysisResultDto
        {
            UrgencyLevel = ai.UrgencyLevel ?? "Routine",
            Analysis = ai.Analysis ?? "",
            SuggestedSpecialty = ai.SuggestedSpecialty ?? "General Medicine",
            PossibleConditions = ai.PossibleConditions ?? Array.Empty<string>(),
            SuggestedActions = ai.SuggestedActions ?? Array.Empty<string>(),
            Disclaimer = "This AI analysis is for informational purposes only and should not replace professional medical advice. Always consult a qualified healthcare provider for diagnosis and treatment.",
            RecommendedHospitalIds = hospitals.ToArray()
        };
    }
}
