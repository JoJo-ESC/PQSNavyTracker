using Microsoft.EntityFrameworkCore;
using PqsTracker.Data;
using PqsTracker.Dtos;
using PqsTracker.Models;

namespace PqsTracker.Services;

public class SignOffService(PqsDbContext db) : ISignOffService
{
    public async Task<ServiceResult<SignOffDto>> CreateAsync(SignOffCreateDto dto)
    {
        // Rule 1: no self-signing. Checked first — it's free, no DB hit needed.
        if (dto.QualifierId == dto.TraineeId)
            return ServiceResult<SignOffDto>.Invalid("A trainee cannot sign off their own line item.");

        var lineItem = await db.LineItems.FindAsync(dto.LineItemId);
        if (lineItem is null)
            return ServiceResult<SignOffDto>.NotFound($"Line item {dto.LineItemId} was not found.");

        var traineeExists = await db.Trainees.AnyAsync(t => t.Id == dto.TraineeId);
        if (!traineeExists)
            return ServiceResult<SignOffDto>.NotFound($"Trainee {dto.TraineeId} was not found.");

        var qualifierExists = await db.Trainees.AnyAsync(t => t.Id == dto.QualifierId);
        if (!qualifierExists)
            return ServiceResult<SignOffDto>.NotFound($"Qualifier {dto.QualifierId} was not found.");

        // Rule 2: qualifier must already hold the qualification this line item belongs to.
        var qualifierHoldsQualification = await db.Trainees
            .Where(t => t.Id == dto.QualifierId)
            .SelectMany(t => t.HeldQualifications)
            .AnyAsync(q => q.Id == lineItem.QualificationId);
        if (!qualifierHoldsQualification)
            return ServiceResult<SignOffDto>.Invalid(
                $"Qualifier {dto.QualifierId} does not hold the qualification this line item belongs to.");

        // Rule 3: no duplicate ACTIVE sign-off for this trainee/line item.
        // A revoked prior sign-off does not block a new one.
        var hasActiveSignOff = await db.SignOffs.AnyAsync(s =>
            s.LineItemId == dto.LineItemId &&
            s.TraineeId == dto.TraineeId &&
            s.RevokedAt == null);
        if (hasActiveSignOff)
            return ServiceResult<SignOffDto>.Invalid(
                "This trainee already has an active sign-off for this line item.");

        var signOff = new SignOff
        {
            LineItemId = dto.LineItemId,
            TraineeId = dto.TraineeId,
            QualifierId = dto.QualifierId,
            SignedAt = DateTime.UtcNow
        };

        db.SignOffs.Add(signOff);
        await db.SaveChangesAsync();

        return ServiceResult<SignOffDto>.Success(ToDto(signOff));
    }

    public async Task<ServiceResult<SignOffDto>> RevokeAsync(int id, SignOffRevokeDto dto)
    {
        var signOff = await db.SignOffs.FindAsync(id);
        if (signOff is null)
            return ServiceResult<SignOffDto>.NotFound($"Sign-off {id} was not found.");

        if (signOff.RevokedAt is not null)
            return ServiceResult<SignOffDto>.Invalid("This sign-off has already been revoked.");

        // Rule 4: append-only. Set the revocation fields; never remove the row.
        signOff.RevokedAt = DateTime.UtcNow;
        signOff.RevocationReason = dto.Reason;
        await db.SaveChangesAsync();

        return ServiceResult<SignOffDto>.Success(ToDto(signOff));
    }

    private static SignOffDto ToDto(SignOff signOff) => new()
    {
        Id = signOff.Id,
        LineItemId = signOff.LineItemId,
        TraineeId = signOff.TraineeId,
        QualifierId = signOff.QualifierId,
        SignedAt = signOff.SignedAt,
        RevokedAt = signOff.RevokedAt,
        RevocationReason = signOff.RevocationReason
    };
}
