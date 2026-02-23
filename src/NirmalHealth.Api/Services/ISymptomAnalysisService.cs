using NirmalHealth.Api.Models.Symptoms;

namespace NirmalHealth.Api.Services;

public interface ISymptomAnalysisService
{
    Task<SymptomAnalysisResultDto> AnalyzeAsync(SymptomAnalysisRequestDto dto, int? userId, CancellationToken ct = default);
}
