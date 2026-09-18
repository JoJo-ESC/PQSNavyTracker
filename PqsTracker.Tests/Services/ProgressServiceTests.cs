using Microsoft.EntityFrameworkCore;
using PqsTracker.Data;
using PqsTracker.Models;
using PqsTracker.Services;
using Xunit;

namespace PqsTracker.Tests.Services;

public class ProgressServiceTests
{
    private static PqsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PqsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PqsDbContext(options);
    }

    // Two required line items and one optional one, plus a trainee with
    // no sign-offs yet. Individual tests add sign-offs as needed.
    private static async Task<(int qualificationId, int traineeId, int requiredLineItem1Id, int requiredLineItem2Id, int optionalLineItemId)>
        SeedScenarioAsync(PqsDbContext db)
    {
        var required1 = new LineItem { Section = Section.Fundamentals, Number = "101.1", Description = "Required 1" };
        var required2 = new LineItem { Section = Section.Fundamentals, Number = "102.1", Description = "Required 2" };
        var optional = new LineItem { Section = Section.Watchstations, Number = "301.1", Description = "Optional", IsRequired = false };

        var qualification = new Qualification { Name = "Reactor Operator", LineItems = [required1, required2, optional] };
        var qualifier = new Trainee { Name = "Qualifier", HeldQualifications = [qualification] };
        var trainee = new Trainee { Name = "Trainee" };

        db.Qualifications.Add(qualification);
        db.Trainees.AddRange(qualifier, trainee);
        await db.SaveChangesAsync();

        return (qualification.Id, trainee.Id, required1.Id, required2.Id, optional.Id);
    }

    private static async Task AddActiveSignOffAsync(PqsDbContext db, int lineItemId, int traineeId, int qualifierId)
    {
        db.SignOffs.Add(new SignOff
        {
            LineItemId = lineItemId,
            TraineeId = traineeId,
            QualifierId = qualifierId,
            SignedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetProgressAsync_ZeroSignOffs_ReturnsZeroPercent()
    {
        await using var db = CreateContext();
        var (qualificationId, traineeId, _, _, _) = await SeedScenarioAsync(db);
        var service = new ProgressService(db);

        var result = await service.GetProgressAsync(traineeId, qualificationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRequired);
        Assert.Equal(0, result.Value.Completed);
        Assert.Equal(0, result.Value.PercentComplete);
        Assert.False(result.Value.IsComplete);
    }

    [Fact]
    public async Task GetProgressAsync_PartialSignOffs_ReturnsPartialPercent()
    {
        await using var db = CreateContext();
        var (qualificationId, traineeId, required1Id, _, _) = await SeedScenarioAsync(db);
        var qualifierId = (await db.Trainees.FirstAsync(t => t.Name == "Qualifier")).Id;
        await AddActiveSignOffAsync(db, required1Id, traineeId, qualifierId);
        var service = new ProgressService(db);

        var result = await service.GetProgressAsync(traineeId, qualificationId);

        Assert.Equal(1, result.Value!.Completed);
        Assert.Equal(2, result.Value.TotalRequired);
        Assert.Equal(50, result.Value.PercentComplete);
        Assert.False(result.Value.IsComplete);
    }

    [Fact]
    public async Task GetProgressAsync_AllRequiredSignedOff_ReturnsComplete()
    {
        await using var db = CreateContext();
        var (qualificationId, traineeId, required1Id, required2Id, _) = await SeedScenarioAsync(db);
        var qualifierId = (await db.Trainees.FirstAsync(t => t.Name == "Qualifier")).Id;
        await AddActiveSignOffAsync(db, required1Id, traineeId, qualifierId);
        await AddActiveSignOffAsync(db, required2Id, traineeId, qualifierId);
        var service = new ProgressService(db);

        var result = await service.GetProgressAsync(traineeId, qualificationId);

        Assert.Equal(2, result.Value!.Completed);
        Assert.Equal(100, result.Value.PercentComplete);
        Assert.True(result.Value.IsComplete);
        Assert.Empty(result.Value.OutstandingLineItems);
    }

    [Fact]
    public async Task GetProgressAsync_OptionalLineItem_DoesNotBlockCompletion()
    {
        await using var db = CreateContext();
        var (qualificationId, traineeId, required1Id, required2Id, optionalLineItemId) = await SeedScenarioAsync(db);
        var qualifierId = (await db.Trainees.FirstAsync(t => t.Name == "Qualifier")).Id;
        // Sign off both required items, but deliberately leave the
        // optional one un-signed.
        await AddActiveSignOffAsync(db, required1Id, traineeId, qualifierId);
        await AddActiveSignOffAsync(db, required2Id, traineeId, qualifierId);
        var service = new ProgressService(db);

        var result = await service.GetProgressAsync(traineeId, qualificationId);

        Assert.True(result.Value!.IsComplete);
        Assert.Equal(2, result.Value.TotalRequired); // the optional item never entered the count
        Assert.DoesNotContain(result.Value.OutstandingLineItems, li => li.Id == optionalLineItemId);
    }
}
