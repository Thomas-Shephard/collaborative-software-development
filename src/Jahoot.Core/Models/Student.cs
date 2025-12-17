namespace Jahoot.Core.Models;

public class Student : User
{
    public int StudentId { get; init; }
    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return string.Empty;
            }

            var names = Name.Trim().Split(' ');
            if (names.Length > 1)
            {
                return (names[0][0].ToString() + names[^1][0].ToString()).ToUpper();
            }

            return names[0][0].ToString().ToUpper();
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is Student student &&
               UserId == student.UserId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UserId);
    }
    
    private StudentAccountStatus _accountStatus;
    public required StudentAccountStatus AccountStatus
    {
        get => _accountStatus;
        set => SetProperty(ref _accountStatus, value);
    }
    public required IReadOnlyList<Subject> Subjects { get; set; }
}
