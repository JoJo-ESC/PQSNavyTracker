namespace PqsTracker.Models;

public class Trainee
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Many-to-many: qualifications this trainee already holds.
    public List<Qualification> HeldQualifications { get; set; } = [];

    // Both sides of SignOff point back to Trainee, so each direction
    // needs its own navigation property here.
    public List<SignOff> SignOffsReceived { get; set; } = [];
    public List<SignOff> SignOffsGiven { get; set; } = [];
}
