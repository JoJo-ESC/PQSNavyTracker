namespace PqsTracker.Desktop.Models;

// Placeholder shape for Day 1 layout work only. Replaced by the real
// LineItemDto from the API project once HTTP calls are wired up.
public class FakeLineItem
{
    public required string Section { get; set; }
    public required string Number { get; set; }
    public required string Description { get; set; }
    public bool IsRequired { get; set; }
}
