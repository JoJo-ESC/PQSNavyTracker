using System.Net.Http.Json;
using PqsTracker.Desktop.Models;

namespace PqsTracker.Desktop.Services;

// Thin wrapper around HttpClient — one shared instance (not a new
// HttpClient per call, which risks socket exhaustion), and every call
// funnels through EnsureSuccessOrThrowAsync so a rejected request raises
// a message-carrying exception instead of a raw, unreadable HTTP error.
public class PqsApiClient
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("http://localhost:5007/")
    };

    public async Task<List<TraineeSummaryDto>> GetTraineesAsync()
    {
        var response = await _http.GetAsync("api/trainees");
        await EnsureSuccessOrThrowAsync(response);
        return await response.Content.ReadFromJsonAsync<List<TraineeSummaryDto>>() ?? [];
    }

    public async Task<List<QualificationSummaryDto>> GetQualificationsAsync()
    {
        var response = await _http.GetAsync("api/qualifications");
        await EnsureSuccessOrThrowAsync(response);
        return await response.Content.ReadFromJsonAsync<List<QualificationSummaryDto>>() ?? [];
    }

    public async Task<ProgressDto> GetProgressAsync(int traineeId, int qualificationId)
    {
        var response = await _http.GetAsync($"api/trainees/{traineeId}/progress/{qualificationId}");
        await EnsureSuccessOrThrowAsync(response);
        return await response.Content.ReadFromJsonAsync<ProgressDto>()
            ?? throw new InvalidOperationException("Empty response from server.");
    }

    public async Task CreateSignOffAsync(int lineItemId, int traineeId, int qualifierId)
    {
        var response = await _http.PostAsJsonAsync("api/signoffs", new
        {
            LineItemId = lineItemId,
            TraineeId = traineeId,
            QualifierId = qualifierId
        });
        await EnsureSuccessOrThrowAsync(response);
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        // The API returns plain-text error bodies for 400/404 — surface
        // that text directly rather than a generic "request failed."
        var body = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(string.IsNullOrWhiteSpace(body)
            ? $"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}"
            : body);
    }
}
