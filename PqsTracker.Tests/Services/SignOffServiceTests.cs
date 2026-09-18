using Microsoft.EntityFrameworkCore;
using PqsTracker.Data;
using PqsTracker.Dtos;
using PqsTracker.Models;
using PqsTracker.Services;
using Xunit;

namespace PqsTracker.Tests.Services;

public class SignOffServiceTests
{
    // Each test gets its own isolated in-memory database, so tests never
    // see each other's data regardless of run order.
    private static PqsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PqsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PqsDbContext(options);
    }

    // Builds one qualification with one required line item, a qualifier
    // who already holds that qualification, and a trainee who doesn't.
    // Returns the ids each test needs to act against.
    private static async Task<(int qualificationId, int lineItemId, int qualifierId, int traineeId)> SeedBasicScenarioAsync(PqsDbContext db)
    {
        var qualification = new Qualification
        {
            Name = "Reactor Operator",
            LineItems = [new LineItem { Section = Section.Fundamentals, Number = "101.1", Description = "Test line item" }]
        };
        var qualifier = new Trainee { Name = "Qualifier", HeldQualifications = [qualification] };
        var trainee = new Trainee { Name = "Trainee" };

        db.Qualifications.Add(qualification);
        db.Trainees.AddRange(qualifier, trainee);
        await db.SaveChangesAsync();

        return (qualification.Id, qualification.LineItems[0].Id, qualifier.Id, trainee.Id);
    }

    [Fact]
    public async Task CreateAsync_ValidSignOff_Succeeds()
    {
        await using var db = CreateContext();
        var (_, lineItemId, qualifierId, traineeId) = await SeedBasicScenarioAsync(db);
        var service = new SignOffService(db);

        var result = await service.CreateAsync(new SignOffCreateDto
        {
            LineItemId = lineItemId,
            TraineeId = traineeId,
            QualifierId = qualifierId
        });

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.RevokedAt);
    }

    [Fact]
    public async Task CreateAsync_SelfSignOff_IsRejected()
    {
        await using var db = CreateContext();
        var (_, lineItemId, qualifierId, _) = await SeedBasicScenarioAsync(db);
        var service = new SignOffService(db);

        var result = await service.CreateAsync(new SignOffCreateDto
        {
            LineItemId = lineItemId,
            TraineeId = qualifierId,
            QualifierId = qualifierId
        });

        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task CreateAsync_UnqualifiedQualifier_IsRejected()
    {
        await using var db = CreateContext();
        var (_, lineItemId, _, traineeId) = await SeedBasicScenarioAsync(db);
        // A second trainee who holds no qualifications at all.
        var unqualified = new Trainee { Name = "Unqualified" };
        db.Trainees.Add(unqualified);
        await db.SaveChangesAsync();
        var service = new SignOffService(db);

        var result = await service.CreateAsync(new SignOffCreateDto
        {
            LineItemId = lineItemId,
            TraineeId = traineeId,
            QualifierId = unqualified.Id
        });

        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task CreateAsync_DuplicateActiveSignOff_IsRejected()
    {
        await using var db = CreateContext();
        var (_, lineItemId, qualifierId, traineeId) = await SeedBasicScenarioAsync(db);
        var service = new SignOffService(db);
        var dto = new SignOffCreateDto { LineItemId = lineItemId, TraineeId = traineeId, QualifierId = qualifierId };

        await service.CreateAsync(dto);
        var second = await service.CreateAsync(dto);

        Assert.Equal(ServiceErrorType.Validation, second.ErrorType);
    }

    [Fact]
    public async Task CreateAsync_AfterRevoke_DuplicateSignOffIsAllowed()
    {
        await using var db = CreateContext();
        var (_, lineItemId, qualifierId, traineeId) = await SeedBasicScenarioAsync(db);
        var service = new SignOffService(db);
        var dto = new SignOffCreateDto { LineItemId = lineItemId, TraineeId = traineeId, QualifierId = qualifierId };

        var first = await service.CreateAsync(dto);
        await service.RevokeAsync(first.Value!.Id, new SignOffRevokeDto { Reason = "Entered in error" });
        var second = await service.CreateAsync(dto);

        Assert.True(second.IsSuccess);
    }

    [Fact]
    public async Task RevokeAsync_SetsTimestampAndReason_DoesNotDeleteRow()
    {
        await using var db = CreateContext();
        var (_, lineItemId, qualifierId, traineeId) = await SeedBasicScenarioAsync(db);
        var service = new SignOffService(db);
        var created = await service.CreateAsync(new SignOffCreateDto
        {
            LineItemId = lineItemId,
            TraineeId = traineeId,
            QualifierId = qualifierId
        });

        var revoked = await service.RevokeAsync(created.Value!.Id, new SignOffRevokeDto { Reason = "Mistaken entry" });

        Assert.True(revoked.IsSuccess);
        Assert.NotNull(revoked.Value!.RevokedAt);
        Assert.Equal("Mistaken entry", revoked.Value.RevocationReason);
        // The row itself must still exist — revoking is not deleting.
        Assert.Equal(1, await db.SignOffs.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_NonexistentTrainee_ReturnsNotFound()
    {
        await using var db = CreateContext();
        var (_, lineItemId, qualifierId, _) = await SeedBasicScenarioAsync(db);
        var service = new SignOffService(db);

        var result = await service.CreateAsync(new SignOffCreateDto
        {
            LineItemId = lineItemId,
            TraineeId = 9999,
            QualifierId = qualifierId
        });

        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task RevokeAsync_AlreadyRevoked_IsRejected()
    {
        await using var db = CreateContext();
        var (_, lineItemId, qualifierId, traineeId) = await SeedBasicScenarioAsync(db);
        var service = new SignOffService(db);
        var created = await service.CreateAsync(new SignOffCreateDto
        {
            LineItemId = lineItemId,
            TraineeId = traineeId,
            QualifierId = qualifierId
        });
        await service.RevokeAsync(created.Value!.Id, new SignOffRevokeDto { Reason = "First revoke" });

        var secondRevoke = await service.RevokeAsync(created.Value.Id, new SignOffRevokeDto { Reason = "Second revoke" });

        Assert.Equal(ServiceErrorType.Validation, secondRevoke.ErrorType);
    }
}
