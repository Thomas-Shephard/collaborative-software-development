namespace Jahoot.Core.Models;

public class Student : User
{
    public int StudentId { get; init; }
    public bool IsApproved { get; set; }
    public required IReadOnlyList<Subject> Subjects { get; set; }
}
