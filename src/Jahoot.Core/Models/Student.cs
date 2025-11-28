namespace Jahoot.Core.Models;

public class Student : User
{
    public int StudentId { get; init; }
    public required StudentAccountStatus AccountStatus { get; set; }
}
