using System.ComponentModel.DataAnnotations;

namespace PqsTracker.Dtos;

public class TraineeCreateDto
{
    [Required, MaxLength(200)]
    public required string Name { get; set; }

    // Lets you create a trainee who already holds qualifications
    // (e.g. seeding a supervisor), without a separate follow-up call.
    public List<int> HeldQualificationIds { get; set; } = [];
}

public class TraineeSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class TraineeDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<QualificationSummaryDto> HeldQualifications { get; set; } = [];
}
