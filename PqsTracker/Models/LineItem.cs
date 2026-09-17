namespace PqsTracker.Models;

public class LineItem
{
    public int Id { get; set; }

    public int QualificationId { get; set; }
    public Qualification? Qualification { get; set; }

    public Section Section { get; set; }
    public required string Number { get; set; }
    public required string Description { get; set; }
    public bool IsRequired { get; set; } = true;

    public List<SignOff> SignOffs { get; set; } = [];
}
