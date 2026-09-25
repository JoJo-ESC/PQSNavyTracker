using System.ComponentModel.DataAnnotations;

namespace PqsTracker.Dtos;

public class SignOffCreateDto
{
    [Required]
    public int LineItemId { get; set; }

    [Required]
    public int TraineeId { get; set; }

    [Required]
    public int QualifierId { get; set; }
}

public class SignOffRevokeDto
{
    [Required, MaxLength(500)]
    public required string Reason { get; set; }
}

public class SignOffDto
{
    public int Id { get; set; }
    public int LineItemId { get; set; }
    public int TraineeId { get; set; }
    public int QualifierId { get; set; }
    public DateTime SignedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
}

// One row per sign-off event (active or revoked) for a trainee against a
// qualification's line items — the audit trail view, as opposed to
// ProgressDto's "what's still outstanding" view.
public class SignOffAuditDto
{
    public int Id { get; set; }
    public required string Section { get; set; }
    public required string Number { get; set; }
    public required string Description { get; set; }
    public required string QualifierName { get; set; }
    public DateTime SignedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
}
