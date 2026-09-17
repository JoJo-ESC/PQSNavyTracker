using Microsoft.EntityFrameworkCore;
using PqsTracker.Data;
using PqsTracker.Dtos;
using PqsTracker.Models;

namespace PqsTracker.Services;

public class TraineeService(PqsDbContext db) : ITraineeService
{
    public async Task<List<TraineeSummaryDto>> GetAllAsync()
    {
        return await db.Trainees
            .OrderBy(t => t.Name)
            .Select(t => new TraineeSummaryDto
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<TraineeDetailDto>> GetByIdAsync(int id)
    {
        var trainee = await db.Trainees
            .Include(t => t.HeldQualifications)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trainee is null)
            return ServiceResult<TraineeDetailDto>.NotFound($"Trainee {id} was not found.");

        return ServiceResult<TraineeDetailDto>.Success(ToDetailDto(trainee));
    }

    public async Task<ServiceResult<TraineeDetailDto>> CreateAsync(TraineeCreateDto dto)
    {
        var heldQualifications = new List<Qualification>();

        if (dto.HeldQualificationIds.Count > 0)
        {
            heldQualifications = await db.Qualifications
                .Where(q => dto.HeldQualificationIds.Contains(q.Id))
                .ToListAsync();

            // If a client passed an id that doesn't correspond to a real
            // Qualification row, fail loudly rather than silently dropping it.
            var missingIds = dto.HeldQualificationIds.Except(heldQualifications.Select(q => q.Id)).ToList();
            if (missingIds.Count > 0)
                return ServiceResult<TraineeDetailDto>.Invalid(
                    $"Qualification id(s) {string.Join(", ", missingIds)} do not exist.");
        }

        var trainee = new Trainee
        {
            Name = dto.Name,
            HeldQualifications = heldQualifications
        };

        db.Trainees.Add(trainee);
        await db.SaveChangesAsync();

        return ServiceResult<TraineeDetailDto>.Success(ToDetailDto(trainee));
    }

    private static TraineeDetailDto ToDetailDto(Trainee trainee) => new()
    {
        Id = trainee.Id,
        Name = trainee.Name,
        HeldQualifications = trainee.HeldQualifications
            .OrderBy(q => q.Name)
            .Select(q => new QualificationSummaryDto
            {
                Id = q.Id,
                Name = q.Name,
                Description = q.Description
            }).ToList()
    };
}
