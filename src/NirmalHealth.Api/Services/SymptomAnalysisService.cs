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

        var specialtyForDb = ai.SuggestedSpecialty.Replace("General Physician / Internal Medicine", "General Medicine").Trim();
        var hospitals = await _db.Hospitals
            .AsNoTracking()
            .Include(h => h.HospitalSpecialties).ThenInclude(hs => hs.Specialty)
            .Where(h => h.IsActive && h.HospitalSpecialties.Any(hs => hs.Specialty.Name == specialtyForDb || hs.Specialty.Name == ai.SuggestedSpecialty))
            .Select(h => h.Id)
            .Take(10)
            .ToListAsync(ct);
        if (hospitals.Count == 0)
            hospitals = await _db.Hospitals.Where(h => h.IsActive).Select(h => h.Id).Take(5).ToListAsync(ct);

        var log = new SymptomAnalysis
        {
            UserId = userId,
            SymptomsText = dto.SymptomsText,
            Age = dto.Age,
            Gender = dto.Gender,
            MedicalHistory = dto.MedicalHistory,
            SuggestedSpecialty = specialtyForDb,
            PossibleConditionsJson = System.Text.Json.JsonSerializer.Serialize(ai.PossibleConditions),
            RecommendedHospitalIdsJson = System.Text.Json.JsonSerializer.Serialize(hospitals),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.SymptomAnalyses.Add(log);
        await _db.SaveChangesAsync(ct);

        return new SymptomAnalysisResultDto
        {
            UrgencyLevel = ai.UrgencyLevel,
            Analysis = ai.Analysis,
            SuggestedSpecialty = ai.SuggestedSpecialty,
            PossibleConditions = ai.PossibleConditions,
            SuggestedActions = ai.SuggestedActions,
            Disclaimer = "This AI analysis is for informational purposes only and should not replace professional medical advice. Always consult a qualified healthcare provider for diagnosis and treatment.",
            RecommendedHospitalIds = hospitals.ToArray()
        };
    }
}
