namespace PqsTracker.Desktop.Models;

// Mirrors the API's response DTOs. Deliberately not referencing the
// PqsTracker project directly — that would drag ASP.NET Core/EF Core
// dependencies into a desktop client for no reason. Same argument as
// DTOs-vs-entities on the backend: client and server contracts are
// allowed to evolve independently.

public class TraineeSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class QualificationSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class LineItemDto
{
    public int Id { get; set; }
    public string Section { get; set; } = "";
    public string Number { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsRequired { get; set; }
}

public class ProgressDto
{
    public string QualificationName { get; set; } = "";
    public int TotalRequired { get; set; }
    public int Completed { get; set; }
    public double PercentComplete { get; set; }
    public bool IsComplete { get; set; }
    public List<LineItemDto> OutstandingLineItems { get; set; } = [];
}

public class SignOffAuditDto
{
    public int Id { get; set; }
    public string Section { get; set; } = "";
    public string Number { get; set; } = "";
    public string Description { get; set; } = "";
    public string QualifierName { get; set; } = "";
    public DateTime SignedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }

    // Client-only computed properties — not part of the JSON payload, just
    // convenience for the grid so the view doesn't need a converter for
    // simple text formatting.
    public bool IsRevoked => RevokedAt is not null;
    public string StatusText => IsRevoked ? $"Revoked: {RevocationReason}" : "Active";
}
