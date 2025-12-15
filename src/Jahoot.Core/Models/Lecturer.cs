namespace Jahoot.Core.Models;

public class Lecturer : User
{
    public int LecturerId { get; init; }
    public bool IsAdmin { get; set; }
}
