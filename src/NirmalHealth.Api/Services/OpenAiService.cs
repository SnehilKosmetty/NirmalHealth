using System.Text.Json;
using NirmalHealth.Api.Models.Symptoms;

namespace NirmalHealth.Api.Services;

public class OpenAiService : IOpenAiService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenAiService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SymptomAiResult> GetSymptomRecommendationAsync(string symptomsText, int? age, string? gender, string? medicalHistory, CancellationToken ct = default)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new SymptomAiResult(
                UrgencyLevel: "Routine",
                Analysis: "Describe your symptoms and consult a doctor for proper evaluation. This tool provides general guidance only.",
                SuggestedSpecialty: "General Medicine",
                PossibleConditions: new[] { "Please consult a doctor for proper diagnosis." },
                SuggestedActions: new[]
                {
                    "Rest and stay hydrated.",
                    "Monitor your symptoms and note any worsening.",
                    "Seek a consultation with a General Physician if symptoms persist or worsen.",
                    "In case of difficulty breathing, chest pain, or severe pain, go to the emergency department."
                }
            );
        }

        var client = _httpClientFactory.CreateClient(nameof(IOpenAiService));
        var model = _config["OpenAI:SymptomAnalysisModel"] ?? "gpt-4o-mini";
        var prompt = $@"You are a medical triage assistant for a rural healthcare app in India (Nirmal district, Telangana). Based on the following, respond with ONLY a valid JSON object (no markdown, no extra text) with these exact keys:
- ""urgencyLevel"": one of ""Urgent"", ""Moderate"", ""Routine"" (use Urgent for fever >101°F, breathing difficulty, chest pain, severe pain, danger signs; Moderate for persistent or concerning symptoms; Routine for mild/self-limiting).
- ""analysis"": one detailed paragraph (2-4 sentences) explaining possible causes and context for this patient (age, gender), relevant to India/Telangana (e.g. dengue, malaria, typhoid, viral infections). Be informative but non-alarming. Do not diagnose.
- ""suggestedSpecialty"": one of: General Medicine, Emergency, Pediatrics, Cardiology, Dermatology, Gynecology, Orthopedics (use ""General Physician / Internal Medicine"" if General Medicine).
- ""possibleConditions"": array of 1-3 brief condition names for awareness only.
- ""suggestedActions"": array of 5-7 short actionable items: self-care (e.g. rest, hydration, paracetamol as per label), when to see a doctor, when to go to emergency, and what to tell the clinician. Be specific and practical.

Symptoms: {symptomsText}
Age: {age?.ToString() ?? "not provided"}
Gender: {gender ?? "not provided"}
Medical history: {medicalHistory ?? "none"}";

        var request = new
        {
            model,
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 800
        };
        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        req.Headers.Add("Authorization", "Bearer " + apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
        var response = await client.SendAsync(req, ct);
        if (!response.IsSuccessStatusCode)
        {
            return new SymptomAiResult(
                "Routine",
                "Unable to analyze; please describe your symptoms to a doctor for evaluation.",
                "General Medicine",
                new[] { "Please consult a doctor for proper diagnosis." },
                new[] { "Seek a consultation with a General Physician.", "In emergency (breathing difficulty, chest pain, severe pain), go to the nearest hospital." }
            );
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                return FallbackResult();
            var firstChoice = choices[0];
            if (!firstChoice.TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var contentEl))
                return FallbackResult();
            var content = contentEl.GetString()?.Trim() ?? "{}";
            if (content.StartsWith("```")) content = content.Replace("```json", "").Replace("```", "").Trim();
            using var resultDoc = JsonDocument.Parse(content);
            var root = resultDoc.RootElement;
            var urgency = GetString(root, "urgencyLevel") ?? "Routine";
            var analysis = GetString(root, "analysis") ?? "";
            var specialty = GetString(root, "suggestedSpecialty") ?? "General Medicine";
            if (specialty == "General Medicine") specialty = "General Physician / Internal Medicine";
            var conditions = GetStringArray(root, "possibleConditions");
            var actions = GetStringArray(root, "suggestedActions");
            if (actions.Length == 0)
                actions = new[] { "Rest and stay hydrated.", "Consult a doctor if symptoms persist or worsen." };
            return new SymptomAiResult(urgency, analysis, specialty, conditions, actions);
        }
        catch
        {
            return FallbackResult();
        }
    }

    private static SymptomAiResult FallbackResult() => new(
        "Routine",
        "Unable to analyze; please describe your symptoms to a doctor for evaluation.",
        "General Medicine",
        new[] { "Please consult a doctor for proper diagnosis." },
        new[] { "Seek a consultation with a General Physician.", "In emergency (breathing difficulty, chest pain, severe pain), go to the nearest hospital." }
    );

    private static string? GetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) ? p.GetString() : null;

    private static string[] GetStringArray(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var arr)) return Array.Empty<string>();
        var list = new List<string>();
        foreach (var e in arr.EnumerateArray())
        {
            var s = e.GetString();
            if (!string.IsNullOrWhiteSpace(s)) list.Add(s.Trim());
        }
        return list.ToArray();
    }
}
