namespace Jahoot.Core.Models;

public class Student : User
{
    public int StudentId { get; init; }
    private StudentAccountStatus _accountStatus;
    public required StudentAccountStatus AccountStatus
    {
        get => _accountStatus;
        set => SetProperty(ref _accountStatus, value);
    }

    // New Initials property
    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return string.Empty;
            }
            return Name.Trim()[0].ToString().ToUpper();
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
}
