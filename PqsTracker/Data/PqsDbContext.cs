using Microsoft.EntityFrameworkCore;
using PqsTracker.Models;

namespace PqsTracker.Data;

public class PqsDbContext(DbContextOptions<PqsDbContext> options) : DbContext(options)
{
    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<LineItem> LineItems => Set<LineItem>();
    public DbSet<Trainee> Trainees => Set<Trainee>();
    public DbSet<SignOff> SignOffs => Set<SignOff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // LineItem -> Qualification: convention already infers this from
        // QualificationId, but it's spelled out here to pin the delete
        // behavior explicitly rather than rely on EF Core's default.
        modelBuilder.Entity<LineItem>()
            .HasOne(li => li.Qualification)
            .WithMany(q => q.LineItems)
            .HasForeignKey(li => li.QualificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Implicit many-to-many: no join entity class needed, just name
        // the hidden join table EF Core generates.
        modelBuilder.Entity<Trainee>()
            .HasMany(t => t.HeldQualifications)
            .WithMany(q => q.HolderTrainees)
            .UsingEntity(j => j.ToTable("TraineeQualifications"));

        // SignOff -> LineItem: deleting a qualification's line items takes
        // their sign-off history with them.
        modelBuilder.Entity<SignOff>()
            .HasOne(s => s.LineItem)
            .WithMany(li => li.SignOffs)
            .HasForeignKey(s => s.LineItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // SignOff -> Trainee (the two relationships to the same table).
        // Restrict on both: a Trainee row can't be deleted while it still
        // has sign-off history, as either the trainee or the qualifier.
        modelBuilder.Entity<SignOff>()
            .HasOne(s => s.Trainee)
            .WithMany(t => t.SignOffsReceived)
            .HasForeignKey(s => s.TraineeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SignOff>()
            .HasOne(s => s.Qualifier)
            .WithMany(t => t.SignOffsGiven)
            .HasForeignKey(s => s.QualifierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Store the enum as text ("Systems") instead of an int, so the
        // raw SQLite file stays readable.
        modelBuilder.Entity<LineItem>()
            .Property(li => li.Section)
            .HasConversion<string>();
    }
}
