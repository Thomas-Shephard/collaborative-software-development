namespace Jahoot.Core.Models;

public class Subject
{
    public int SubjectId { get; init; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
