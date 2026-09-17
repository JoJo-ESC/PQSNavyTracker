namespace PqsTracker.Models;

public class Qualification
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<LineItem> LineItems { get; set; } = [];

    // The other side of Trainee.HeldQualifications.
    public List<Trainee> HolderTrainees { get; set; } = [];
}
