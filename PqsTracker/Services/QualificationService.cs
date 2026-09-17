using Microsoft.EntityFrameworkCore;
using PqsTracker.Data;
using PqsTracker.Dtos;
using PqsTracker.Models;

namespace PqsTracker.Services;

public class QualificationService(PqsDbContext db) : IQualificationService
{
    public async Task<List<QualificationSummaryDto>> GetAllAsync()
    {
        // Projecting straight into the DTO via Select() means EF Core's
        // generated SQL only selects the columns the DTO needs — it never
        // materializes full Qualification entities just to discard most of
        // their data.
        return await db.Qualifications
            .OrderBy(q => q.Name)
            .Select(q => new QualificationSummaryDto
            {
                Id = q.Id,
                Name = q.Name,
                Description = q.Description
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<QualificationDetailDto>> GetByIdAsync(int id)
    {
        var qualification = await db.Qualifications
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (qualification is null)
            return ServiceResult<QualificationDetailDto>.NotFound($"Qualification {id} was not found.");

        return ServiceResult<QualificationDetailDto>.Success(ToDetailDto(qualification));
    }

    public async Task<QualificationDetailDto> CreateAsync(QualificationCreateDto dto)
    {
        var qualification = new Qualification
        {
            Name = dto.Name,
            Description = dto.Description,
            LineItems = dto.LineItems.Select(li => new LineItem
            {
                Section = li.Section,
                Number = li.Number,
                Description = li.Description,
                IsRequired = li.IsRequired
            }).ToList()
        };

        db.Qualifications.Add(qualification);
        await db.SaveChangesAsync();

        return ToDetailDto(qualification);
    }

    public async Task<ServiceResult<QualificationDetailDto>> UpdateAsync(int id, QualificationUpdateDto dto)
    {
        var qualification = await db.Qualifications
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (qualification is null)
            return ServiceResult<QualificationDetailDto>.NotFound($"Qualification {id} was not found.");

        qualification.Name = dto.Name;
        qualification.Description = dto.Description;
        await db.SaveChangesAsync();

        return ServiceResult<QualificationDetailDto>.Success(ToDetailDto(qualification));
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var qualification = await db.Qualifications.FindAsync(id);
        if (qualification is null)
            return ServiceResult.NotFound($"Qualification {id} was not found.");

        db.Qualifications.Remove(qualification);
        await db.SaveChangesAsync();

        return ServiceResult.Success();
    }

    private static QualificationDetailDto ToDetailDto(Qualification qualification) => new()
    {
        Id = qualification.Id,
        Name = qualification.Name,
        Description = qualification.Description,
        LineItems = qualification.LineItems
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
    };
}
