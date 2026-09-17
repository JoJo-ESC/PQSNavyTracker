using System.ComponentModel.DataAnnotations;
using PqsTracker.Models;

namespace PqsTracker.Dtos;

// Inbound shape for a line item nested inside a qualification-create request.
public class LineItemCreateDto
{
    [Required]
    public Section Section { get; set; }

    [Required, MaxLength(20)]
    public required string Number { get; set; }

    [Required, MaxLength(500)]
    public required string Description { get; set; }

    public bool IsRequired { get; set; } = true;
}

public class QualificationCreateDto
{
    [Required, MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public List<LineItemCreateDto> LineItems { get; set; } = [];
}

// Only Name/Description are editable after creation — line items aren't
// part of this DTO, matching the spec's "update name/description" scope.
public class QualificationUpdateDto
{
    [Required, MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}

// Outbound shapes.
public class LineItemDto
{
    public int Id { get; set; }
    public Section Section { get; set; }
    public required string Number { get; set; }
    public required string Description { get; set; }
    public bool IsRequired { get; set; }
}

// Lightweight — used for the list endpoint.
public class QualificationSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

// Full shape — used for the single-item endpoint, includes line items.
public class QualificationDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<LineItemDto> LineItems { get; set; } = [];
}
