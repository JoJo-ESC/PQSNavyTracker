namespace PqsTracker.Dtos;

public class ProgressDto
{
    public required string QualificationName { get; set; }
    public int TotalRequired { get; set; }
    public int Completed { get; set; }
    public double PercentComplete { get; set; }
    public bool IsComplete { get; set; }
    public List<LineItemDto> OutstandingLineItems { get; set; } = [];
}
