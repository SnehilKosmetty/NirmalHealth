namespace NirmalHealth.Api.Services;

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string? entityType, string? entityId, string? oldValuesJson, string? newValuesJson, string? ipAddress, CancellationToken ct = default);
}
