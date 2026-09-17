namespace PqsTracker.Models;

public class SignOff
{
    public int Id { get; set; }

    public int LineItemId { get; set; }
    public LineItem? LineItem { get; set; }

    // The trainee being signed off.
    public int TraineeId { get; set; }
    public Trainee? Trainee { get; set; }

    // The supervisor doing the signing. Also a Trainee row (per the spec,
    // anyone qualified can qualify others) — a different relationship to
    // the same table, so it needs its own foreign key + navigation.
    public int QualifierId { get; set; }
    public Trainee? Qualifier { get; set; }

    public DateTime SignedAt { get; set; }

    // Append-only: never delete a SignOff row. "Removing" one means
    // setting these two instead, leaving RevokedAt null for active sign-offs.
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
}
