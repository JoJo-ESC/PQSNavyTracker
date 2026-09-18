using Microsoft.EntityFrameworkCore;
using PqsTracker.Data;
using PqsTracker.Dtos;

namespace PqsTracker.Services;

public class ProgressService(PqsDbContext db) : IProgressService
{
    public async Task<ServiceResult<ProgressDto>> GetProgressAsync(int traineeId, int qualificationId)
    {
        var traineeExists = await db.Trainees.AnyAsync(t => t.Id == traineeId);
        if (!traineeExists)
            return ServiceResult<ProgressDto>.NotFound($"Trainee {traineeId} was not found.");

        var qualification = await db.Qualifications
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == qualificationId);
        if (qualification is null)
            return ServiceResult<ProgressDto>.NotFound($"Qualification {qualificationId} was not found.");

        // Rule 5: completion is computed here, on every request, from the
        // current set of active sign-offs — never read from a stored flag.
        var requiredLineItems = qualification.LineItems.Where(li => li.IsRequired).ToList();
        var requiredIds = requiredLineItems.Select(li => li.Id).ToList();

        var completedIds = (await db.SignOffs
            .Where(s => s.TraineeId == traineeId && s.RevokedAt == null && requiredIds.Contains(s.LineItemId))
            .Select(s => s.LineItemId)
            .ToListAsync())
            .ToHashSet();

        var outstanding = requiredLineItems.Where(li => !completedIds.Contains(li.Id)).ToList();
        var totalRequired = requiredLineItems.Count;
        var completed = totalRequired - outstanding.Count;

        return ServiceResult<ProgressDto>.Success(new ProgressDto
        {
            QualificationName = qualification.Name,
            TotalRequired = totalRequired,
            Completed = completed,
            PercentComplete = totalRequired == 0 ? 100 : Math.Round(100.0 * completed / totalRequired, 1),
            IsComplete = completed == totalRequired,
            OutstandingLineItems = outstanding
                .OrderBy(li => li.Section)
                .ThenBy(li => li.Number)
                .Select(li => new LineItemDto
                {
                    Id = li.Id,
                    Section = li.Section,
                    Number = li.Number,
                    Description = li.Description,
                    IsRequired = li.IsRequired
                }).ToList()
        });
    }
}
